import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = async () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.checked()) {
    await firstValueFrom(auth.checkSession());
  }

  return auth.isLoggedIn() ? true : router.createUrlTree(['/login']);
};
