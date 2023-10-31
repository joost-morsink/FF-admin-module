import {Injectable} from "@angular/core";
import {HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest} from "@angular/common/http";
import {Observable} from "rxjs";
import {CurrentBranch} from "./currentBranch";

@Injectable()
export class BranchInterceptor implements HttpInterceptor {
  constructor(private currentBranch: CurrentBranch) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    req = req.clone({
      headers: req.headers.set('Branch', this.currentBranch.getBranchName())
    });
    console.log(req);
    return next.handle(req);
  }
}
