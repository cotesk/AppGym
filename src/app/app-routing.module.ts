import { NuevosUsuariosComponent } from './Components/nuevos-usuarios/nuevos-usuarios.component';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './Components/login/login.component';
import { AuthLoginGuard } from './Services/auth-login.guard';
import { LayoutComponent } from './Components/layout/layout.component';

import { SolicitarRestablecimientoComponent } from './Components/solicitar-restablecimiento/solicitar-restablecimiento.component';
import { RestablecerContrasenaComponent } from './Components/restablecer-contrasena/restablecer-contrasena.component';
 import { MenuPresentacionComponent } from './Components/menu-presentacion/menu-presentacion.component';
import { PresentacionComponent } from './Components/presentacion/presentacion.component';
import { ActivarCuentaComponent } from './Components/activar-cuenta/activar-cuenta.component';
import { ConsularMembresiaComponent } from './Components/consular-membresia/consular-membresia.component';

//import { MenuPresentacionComponent } from './Components/layout/Pages/menu-presentacion/menu-presentacion.component';


const routes: Routes = [


  { path: "", component: LoginComponent, pathMatch: "full" },
  //  { path: 'pages/cards', component: ProductoCardComponent, pathMatch: "full" },
  // { path: 'solicitar-restablecimiento', component: SolicitarRestablecimientoComponent, pathMatch: "full" },
   { path: 'login', component: LoginComponent},
  // { path: 'nuevosUsuarios', component: NuevosUsuariosComponent},
  { path: 'activar-cuenta', component: ActivarCuentaComponent },
   { path: 'restablecer-contrasena', component: RestablecerContrasenaComponent },

  //funcional
  { path: "menu",
  component: MenuPresentacionComponent,
  children: [
    // { path: "", redirectTo: "solicitar-restablecimiento", pathMatch: "full" },
    // { path: "solicitar-restablecimiento", component: SolicitarRestablecimientoComponent },

    { path: "", redirectTo: "consultar", pathMatch: "full" },
    { path: "consultar", component: ConsularMembresiaComponent },

  ]
},
  {
    path: 'pages', loadChildren: () => import("./Components/layout/layout.module").then(m => m.LayoutModule),
    canActivate: [AuthLoginGuard],
  },

  { path: '**', redirectTo: 'login' },
  // { path: 'preview/:id', component: PreviewComponent, pathMatch: 'full' },
  // {path:'pages',loadChildren:()=>import("./Components/layout/layout.module").then(m => m.LayoutModule)},
  //  {path:'**',redirectTo:'login',pathMatch:"full"},




];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
