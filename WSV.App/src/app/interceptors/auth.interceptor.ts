import { inject } from "@angular/core";
import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { catchError, throwError } from "rxjs";

import { AuthService } from "../services/auth-service";
import { RefreshService } from "../services/refresh-service";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const auth = inject(AuthService);
    const refresh = inject(RefreshService);
    const token = auth.token();

    const requestToSend = token
        ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
        : req;

    return next(requestToSend).pipe(
        catchError((err: unknown) => {
            if (err instanceof HttpErrorResponse && err.status === 401) {
                auth.clearAuth();
            }
            return throwError(() => err);
        })
    )
}