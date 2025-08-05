import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { PublicService } from '../../services/public.service';
import { first } from 'rxjs';

@Component({
  selector: 'app-public',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatIconModule, MatInputModule],
  templateUrl: './public.component.html',
  styleUrl: './public.component.scss',
})
export class PublicComponent {
  private formBuilder = inject(FormBuilder);
  private publicService = inject(PublicService);

  private passVisible = signal(false);
  protected passType = computed(() => (this.passVisible() ? 'text' : 'password'));
  protected passIcon = computed(() => `visibility_${this.passVisible() ? 'off' : 'on'}`);

  protected form = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  protected togglePassVisibility(event: MouseEvent) {
    event.stopPropagation();
    this.passVisible.update((passVisible) => !passVisible);
  }

  protected submit() {
    if (this.form.invalid) return;

    const req = {
      email: this.form.value.email!,
      password: this.form.value.password!,
    };

    this.publicService
      .login(req)
      .pipe(first())
      .subscribe({
        next: (res) => {
          console.log('login success', res);
        },
        error: () => {
          this.form.reset();
          this.form.markAsPristine();
        },
      });
  }
}
