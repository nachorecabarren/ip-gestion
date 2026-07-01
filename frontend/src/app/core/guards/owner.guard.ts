import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../services/auth.service';

/** Requires an authenticated OWNER. Employees are bounced to the dashboard. */
export const ownerGuard: CanActivateFn = async () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.checked()) {
    await firstValueFrom(auth.checkSession());
  }

  if (!auth.isLoggedIn()) return router.createUrlTree(['/login']);
  return auth.isOwner() ? true : router.createUrlTree(['/dashboard']);
};
