import { NgModule } from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {ConfigPanelComponent} from "./pages/config-panel/config-panel.component";

const routes: Routes = [
  {
    component: ConfigPanelComponent,
    path: 'config',
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
