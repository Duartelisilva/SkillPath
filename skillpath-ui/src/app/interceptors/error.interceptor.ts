import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ErrorHandlerService } from '../services/error-handler.service';

/**
 * Global HTTP error interceptor
 * Catches all HTTP errors and delegates to ErrorHandlerService
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorHandler = inject(ErrorHandlerService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only handle errors that haven't been handled by the component
      // Components can set a custom header to skip global error handling
      if (!req.headers.has('X-Skip-Error-Interceptor')) {
        errorHandler.handleHttpError(error);
      }

      return throwError(() => error);
    })
  );
};