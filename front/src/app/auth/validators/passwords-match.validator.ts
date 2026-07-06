import { AbstractControl, ValidationErrors } from '@angular/forms';

export function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
	const password = control.get('password');
	const confirmPassword = control.get('confirmPassword');

	if (!password || !confirmPassword) return null;

	if (confirmPassword.value !== password.value) {
		confirmPassword.setErrors({ ...confirmPassword.errors, mismatch: true });
	} else if (confirmPassword.hasError('mismatch')) {
		const { mismatch, ...errors } = confirmPassword.errors ?? {};
		confirmPassword.setErrors(Object.keys(errors).length ? errors : null);
	}

	return null;
}
