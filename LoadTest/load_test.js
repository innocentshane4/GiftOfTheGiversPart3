```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 5 },
        { duration: '1m', target: 10 },
        { duration: '30s', target: 0 },
    ],
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<2000'],
        checks: ['rate>0.95'],
    },
};

export default function () {
    const url = __ENV.BASE_URL || 'https://localhost:7099/';

    const response = http.get(url);

    check(response, {
        'homepage returns HTTP 200': function (r) {
            return r.status === 200;
        },
        'response time below 2 seconds': function (r) {
            return r.timings.duration < 2000;
        },
    });

    sleep(1);
}
```