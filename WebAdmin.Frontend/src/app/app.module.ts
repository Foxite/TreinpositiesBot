import {NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {TopBarModule} from "./components/top-bar/top-bar.module";
import {MainModule} from "./components/main/main.module";
import {HttpClientModule} from "@angular/common/http";
import {ReactiveFormsModule} from "@angular/forms";
import { GuildsComponent } from './components/guilds/guilds.component';
import { GuildComponent } from './components/guild/guild.component';

@NgModule({
  declarations: [
    AppComponent,
    GuildsComponent,
    GuildComponent
  ],
  imports: [
    HttpClientModule,
    ReactiveFormsModule,
    BrowserModule,
    AppRoutingModule,
    TopBarModule,
    MainModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
}
