import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {AppModule} from "../../app.module";
import {filter} from "rxjs";
import {SecurityService} from "../../services/security/security.service";
import {User} from "../../services/security/user";

@Component({
	selector: 'app-top-bar',
	templateUrl: './top-bar.component.html',
	styleUrls: ['./top-bar.component.scss']
})
export class TopBarComponent implements OnInit {
  @Input()  username!: string | "";
  @Output() usernameChange = new EventEmitter<string>();

  constructor(public security: SecurityService) {
  }

  ngOnInit(): void {
    console.log("hey.");
    this.refreshUser(this.security.currentUser());
    console.log(this.security);
    const observable = this.security.userUpdated();

    console.log(observable);
    const self = this;
    observable.subscribe({
      complete() {
        console.error("completeE????");
      },
      error(err) {
        console.error("errroror????");
        console.error(err);
      },
      next(user) {
        console.log("oh hi!");
        console.log(user);
        self.refreshUser(user);
      }
    });
    console.log(observable);
  }

  private refreshUser(user: User | null) {
    if (user) {
      this.username = user.name;
    } else {
      this.username = "";
    }
    this.usernameChange.emit(this.username);
  }
}
