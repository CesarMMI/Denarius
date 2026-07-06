import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { IdCard as IdCardIcon } from '@primeicons/angular/id-card';
import { Key as KeyIcon } from '@primeicons/angular/key';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';
import { AuthService } from '../../services/auth.service';

@Component({
	selector: 'app-login',
	templateUrl: './login.page.html',
	styleUrl: './login.page.scss',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [
		ReactiveFormsModule,
		RouterLink,
		KeyIcon,
		IdCardIcon,
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
export class LoginPage {
	private readonly router = inject(Router);
	private readonly authService = inject(AuthService);
	private readonly submitted = signal(false);

	protected readonly registered = inject(ActivatedRoute).snapshot.queryParamMap.has('registered');
	protected readonly errorMessage = signal<string | null>(null);

	protected readonly form = inject(NonNullableFormBuilder).group({
		email: ['', [Validators.required, Validators.email]],
		password: ['', [Validators.required, Validators.minLength(3)]],
	});

	protected submit() {
		this.submitted.set(true);
		this.errorMessage.set(null);

		if (this.form.invalid) {
			this.form.markAllAsTouched();
			return;
		}

		this.authService.login(this.form.getRawValue()).subscribe({
			next: () => this.router.navigateByUrl('/accounts'),
			error: (error: unknown) => {
				const detail = error instanceof HttpErrorResponse ? (error.error?.detail as string | undefined) : undefined;
				this.errorMessage.set(detail ?? 'Credenciais inválidas.');
			},
		});
	}
}
