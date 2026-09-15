import { HttpInterceptorFn } from '@angular/common/http';
import { delay } from 'rxjs';

export const delayInterceptor: HttpInterceptorFn = (req, next) => {
	return next(req).pipe(
		delay(Math.random() * 1000 + 500)
	);
};
