import { inject } from "@angular/core";
import { Router, CanMatchFn } from "@angular/router";
import { map } from "rxjs/operators";
import { AuthService } from "../services/auth.service";

export const authGuard: CanMatchFn = (route, segments) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuth().pipe(
    map((isAuthenticated) => {
      if (!isAuthenticated) {
        router.navigate(["/login"]);
        return false;
      }
      return true;
    }),
  );
};
