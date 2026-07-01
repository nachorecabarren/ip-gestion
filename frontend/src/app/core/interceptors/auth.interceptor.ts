import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  // withCredentials sends the HTTP-only cookie on every request
  return next(req.clone({ withCredentials: true })).pipe(
    catchError(err => {
      if (err.status === 401 && !req.url.includes('/auth/')) {
        router.navigate(['/login']);
      }
      return throwError(() => err);
    })
  );
};
