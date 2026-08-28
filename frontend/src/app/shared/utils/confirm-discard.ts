import { FormGroup } from '@angular/forms';
import { ConfirmService } from '../services/confirm.service';

/**
 * Resolves true immediately if the form has no unsaved user input.
 * Otherwise asks the user to confirm discarding it before closing a modal.
 */
export function confirmDiscard(confirm: ConfirmService, form: FormGroup): Promise<boolean> {
  if (!form.dirty) return Promise.resolve(true);
  return confirm.open('¿Descartar los cambios sin guardar?');
}
