import http from 'k6/http';
import { sleep, check } from 'k6';

export const options = {
  stages: [
    { duration: '1m', target: 1 },
    { duration: '1m', target: 5 },
    { duration: '1m', target: 10 },
    { duration: '1m', target: 20 },
    { duration: '1m', target: 30 },
  ],
};

export default function() {
  let res = http.get('http://pythonparser.jonaskaad.com');
  check(res, { "status is 200": (res) => res.status === 200 });
  sleep(1);
}
