import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { IdCard as IdCardIcon } from '@primeicons/angular/id-card';
import { Key as KeyIcon } from '@primeicons/angular/key';
import { User as UserIcon } from '@primeicons/angular/user';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';
import { AuthService } from '../../services/auth.service';
import { passwordsMatchValidator } from '../../validators/passwords-match.validator';

@Component({
	selector: 'app-register',
	templateUrl: './register.page.html',
	styleUrl: './register.page.scss',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [
		ReactiveFormsModule,
		RouterLink,
		IdCardIcon,
		KeyIcon,
		UserIcon,
		ButtonDirective,
		Card,
		IconField,
		InputIcon,
		InputText,
		Message,
		Password,
		DividerModule,
	],
})
export class RegisterPage {
	private readonly router = inject(Router);
	private readonly authService = inject(AuthService);
	protected readonly submitted = signal(false);
	protected readonly errorMessage = signal<string | null>(null);

	protected readonly form = inject(NonNullableFormBuilder).group(
		{
			name: ['', [Validators.required]],
			email: ['', [Validators.required, Validators.email]],
			password: ['', [Validators.required, Validators.minLength(3)]],
			confirmPassword: ['', [Validators.required]],
		},
		{ validators: passwordsMatchValidator },
	);

	protected submit() {
		this.submitted.set(true);
		this.errorMessage.set(null);

		if (this.form.invalid) {
			this.form.markAllAsTouched();
			return;
		}

		const { name, email, password } = this.form.getRawValue();

		this.authService.register({ name, email, password }).subscribe({
			next: () => this.router.navigate(['/auth/login'], { queryParams: { registered: true } }),
			error: (error: unknown) => {
				const detail = error instanceof HttpErrorResponse ? (error.error?.detail as string | undefined) : undefined;
				this.errorMessage.set(detail ?? 'Não foi possível criar sua conta.');
			},
		});
	}
}
