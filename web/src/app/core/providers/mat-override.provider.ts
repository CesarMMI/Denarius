import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { MAT_CARD_CONFIG, MatCardConfig } from '@angular/material/card';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldDefaultOptions } from '@angular/material/form-field';

export const provideMatOverride = (): EnvironmentProviders =>
  makeEnvironmentProviders([
    {
      provide: MAT_CARD_CONFIG,
      useValue: { appearance: 'outlined' } as MatCardConfig,
    },
    {
      provide: MAT_FORM_FIELD_DEFAULT_OPTIONS,
      useValue: { appearance: 'outline', subscriptSizing: 'dynamic' } as MatFormFieldDefaultOptions,
    },
  ]);
