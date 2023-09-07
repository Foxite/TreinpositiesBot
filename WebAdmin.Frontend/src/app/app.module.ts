import {NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {TopBarModule} from "./components/top-bar/top-bar.module";
import {HttpClientModule} from "@angular/common/http";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {SecurityModule} from "./services/security/security.module";
import {DiscordService} from "./services/discord/discord.service";
import {ConfigPanelComponent} from './pages/config-panel/config-panel.component';
import {LevelSelectorComponent} from './components/level-selector/level-selector.component';
import {ConfigFormComponent} from './components/config-form/config-form.component';
import {RootLevelsComponent} from "./components/root-levels/root-levels.component";

@NgModule({
  declarations: [
    AppComponent,
    RootLevelsComponent,
    LevelSelectorComponent,
    ConfigFormComponent,
    ConfigPanelComponent,
  ],
  imports: [
    SecurityModule,
    HttpClientModule,
    ReactiveFormsModule,
    BrowserModule,
    AppRoutingModule,
    TopBarModule,
    FormsModule,
  ],
  providers: [
    DiscordService
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
