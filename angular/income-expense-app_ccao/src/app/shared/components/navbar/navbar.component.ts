import { Component, OnInit } from "@angular/core";

import { Store } from "@ngrx/store";
import { Subscription } from "rxjs";
import { AppState } from "../../../app.reducer";

@Component({
  selector: "app-navbar",
  templateUrl: "./navbar.component.html",
  styles: [],
})
export class NavbarComponent implements OnInit {
  public userName: string = "";
  private userSubs!: Subscription;

  constructor(private store: Store<AppState>) {}

  ngOnInit() {
    this.userSubs = this.store
      .select(state => state.user)
      .subscribe(({ user }) => (this.userName = user?.name || ""));
  }

  ngOnDestroy(): void {
    this.userSubs?.unsubscribe();
  }
}
