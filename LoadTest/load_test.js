// =============================================================================
// Gift of the Givers — k6 Load Test
// =============================================================================
//
// This script simulates 5 concurrent virtual users (VUs) exercising the
// donation flow over 30 seconds.
//
// PREREQUISITES
//   Install k6:  https://k6.io/docs/getting-started/installation/
//   Windows:     winget install k6  (or Chocolatey: choco install k6)
//   macOS:       brew install k6
//
// HOW TO RUN
//   1. Start the application locally (or point BASE_URL at Azure).
//   2. Open a terminal in this folder.
//   3. Run:  k6 run load_test.js
//
//   To run against the deployed Azure site:
//     k6 run --env BASE_URL=https://giftofthegivers.azurewebsites.net load_test.js
//
// EXPECTED RESULTS (passing thresholds)
//   - http_req_failed   < 1%   (fewer than 1% of requests should fail)
//   - http_req_duration < 2 s  (95th-percentile response time under 2 seconds)
// =============================================================================

import http   from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------
const BASE_URL = __ENV.BASE_URL || 'https://localhost:7001';

export const options = {
    // 5 concurrent virtual users for 30 seconds
    vus:      5,
    duration: '30s',

    thresholds: {
        // Less than 1 % of requests may fail
        http_req_failed:   ['rate<0.01'],
        // 95 % of requests must complete within 2 000 ms
        http_req_duration: ['p(95)<2000'],
    },
};

// ---------------------------------------------------------------------------
// Custom metrics
// ---------------------------------------------------------------------------
const donateFormLoadTime = new Trend('donate_form_load_ms');
const homePageLoadTime   = new Trend('home_page_load_ms');
const errorRate          = new Rate('error_rate');

// ---------------------------------------------------------------------------
// Default function: executed once per VU iteration
// ---------------------------------------------------------------------------
export default function () {

    // ---- Step 1: Load the home page ----------------------------------------
    const homeRes = http.get(`${BASE_URL}/`, {
        tags: { page: 'home' },
    });

    homePageLoadTime.add(homeRes.timings.duration);

    const homeOk = check(homeRes, {
        'Home page — status 200':  (r) => r.status === 200,
        'Home page — has content': (r) => r.body.includes('Gift of the Givers'),
    });
    errorRate.add(!homeOk);

    sleep(0.5);

    // ---- Step 2: Load the Projects listing ---------------------------------
    const projectsRes = http.get(`${BASE_URL}/Projects`, {
        tags: { page: 'projects' },
    });

    check(projectsRes, {
        'Projects page — status 200': (r) => r.status === 200,
    });

    sleep(0.5);

    // ---- Step 3: Load the Donate form --------------------------------------
    const donateRes = http.get(`${BASE_URL}/Donations/Donate`, {
        tags: { page: 'donate_form' },
    });

    donateFormLoadTime.add(donateRes.timings.duration);

    const donateOk = check(donateRes, {
        'Donate form — status 200':    (r) => r.status === 200,
        'Donate form — form rendered': (r) =>
            r.body.includes('FullName') || r.body.includes('Full name'),
    });
    errorRate.add(!donateOk);

    sleep(0.5);

    // ---- Step 4: Extract CSRF token and POST an anonymous donation ---------
    // k6 does not execute JavaScript, so we parse the hidden anti-forgery
    // token from the raw HTML using a regex.
    const tokenMatch = donateRes.body.match(
        /name="__RequestVerificationToken"\s+[^>]*value="([^"]+)"/
    );

    if (tokenMatch) {
        const csrfToken = tokenMatch[1];

        const postRes = http.post(
            `${BASE_URL}/Donations/Donate`,
            {
                '__RequestVerificationToken': csrfToken,
                'FullName':     'Load Test User',
                'Email':        'loadtest@example.com',
                'CustomAmount': '100',
                'Currency':     'ZAR',
                'DonationType': 'OneTime',
                'IsAnonymous':  'true',
            },
            {
                redirects: 5,           // follow the redirect to Confirmation
                tags: { page: 'donate_post' },
            }
        );

        check(postRes, {
            'Donate POST — redirected to confirmation': (r) =>
                r.url.includes('/Confirmation') || r.status === 200,
            'Donate POST — reference number present':  (r) =>
                r.body.includes('GG-'),
        });
    }

    sleep(1);
}

// ---------------------------------------------------------------------------
// Summary hook (printed after the test run)
// ---------------------------------------------------------------------------
export function handleSummary(data) {
    console.log('\n=== Gift of the Givers Load Test Summary ===');
    console.log(`Virtual users:          ${options.vus}`);
    console.log(`Duration:               ${options.duration}`);
    console.log(`Total requests:         ${data.metrics.http_reqs?.values?.count ?? 'N/A'}`);
    console.log(
        `95th-pct response time: ` +
        `${(data.metrics.http_req_duration?.values?.['p(95)'] ?? 0).toFixed(0)} ms`
    );
    console.log(
        `Failure rate:           ` +
        `${((data.metrics.http_req_failed?.values?.rate ?? 0) * 100).toFixed(2)} %`
    );
    console.log('============================================\n');

    return {
        // Write a JSON summary to disk for CI artefact upload
        'load_test_results.json': JSON.stringify(data, null, 2),
        // Also print to stdout
        stdout: '\nSee load_test_results.json for full metrics.\n',
    };
}
