import {Component, OnInit} from '@angular/core';
import {SecurityService} from "./services/security/security.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'webadmin-frontend';

  constructor(private securityService: SecurityService) {
    this.securityService.setup();
  }

  ngOnInit(): void {
    this.securityService.login();
  }
}
