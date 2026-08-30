import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../services/auth.service';

/** Keeps the landing page for logged-out visitors; sends logged-in users straight to the app. */
export const guestGuard: CanActivateFn = async () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.checked()) {
    await firstValueFrom(auth.checkSession());
  }

  return auth.isLoggedIn() ? router.createUrlTree(['/dashboard']) : true;
};
