import { ApplicationConfig, provideZoneChangeDetection, DEFAULT_CURRENCY_CODE, LOCALE_ID } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { registerLocaleData } from '@angular/common';
import localeIn from '@angular/common/locales/en-IN';

import { routes } from './app.routes';
import { jwtInterceptor } from './auth/interceptors/jwt.interceptor';

registerLocaleData(localeIn);

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([jwtInterceptor])),
    provideAnimationsAsync(),
    { provide: DEFAULT_CURRENCY_CODE, useValue: 'INR' },
    { provide: LOCALE_ID, useValue: 'en-IN' }
  ]
};
