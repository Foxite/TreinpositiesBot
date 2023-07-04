import {NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {TopBarModule} from "./components/top-bar/top-bar.module";
import {HttpClientModule} from "@angular/common/http";
import {ReactiveFormsModule} from "@angular/forms";
import { GuildsComponent } from './components/guilds/guilds.component';
import { GuildComponent } from './components/guild/guild.component';
import {SecurityModule} from "./services/security/security.module";
import {DiscordService} from "./services/discord.service";

@NgModule({
  declarations: [
    AppComponent,
    GuildsComponent,
    GuildComponent
  ],
  imports: [
    SecurityModule,
    HttpClientModule,
    ReactiveFormsModule,
    BrowserModule,
    AppRoutingModule,
    TopBarModule,
  ],
  providers: [
    DiscordService
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
