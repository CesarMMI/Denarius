import { HttpContext, HttpContextToken } from '@angular/common/http';

export const SKIP_AUTH = new HttpContextToken<boolean>(() => false);

export class SkipAuthContext extends HttpContext {
  constructor(skipAuth: boolean = true) {
    super();
    this.set(SKIP_AUTH, skipAuth);
  }
}
