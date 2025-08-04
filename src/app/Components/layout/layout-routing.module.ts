import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from './layout.component';
import { AuthLoginGuard } from '../../Services/auth-login.guard';
import { UsuarioComponent } from './Pages/usuario/usuario.component';
import { CajaComponent } from './Pages/caja/caja.component';
import { MenuComponent } from './Pages/menu/menu.component';
import { EmpresaComponent } from './Pages/empresa/empresa.component';
import { ColoresComponent } from './Pages/colores/colores.component';
import { ContenidoComponent } from './Pages/contenido/contenido.component';
import { DashBoardComponent } from './Pages/dashboard/dashboard.component';
import { MembresiaComponent } from './Pages/membresia/membresia.component';
import { AsignarMembresiaComponent } from './Pages/asignar-membresia/asignar-membresia.component';
import { AsistenciaComponent } from './Pages/asistencia/asistencia.component';
import { AsignarEntrenadoresComponent } from './Pages/asignar-entrenadores/asignar-entrenadores.component';
import { PagosComponent } from './Pages/pagos/pagos.component';
import { HistorialDePagosComponent } from './Pages/historial-de-pagos/historial-de-pagos.component';

const routes: Routes = [{

  path: "",
  component: LayoutComponent,
  canActivate: [AuthLoginGuard],
  children: [

    { path: 'usuarios', component: UsuarioComponent },
    { path: 'caja', component: CajaComponent },
    { path: 'colores', component: ColoresComponent },
    { path: 'empresa', component: EmpresaComponent },
    { path: 'menu', component: MenuComponent },
    { path: 'contenido', component: ContenidoComponent },
    { path: 'dashboard', component: DashBoardComponent },
    { path: 'RegistrarMembresia', component: MembresiaComponent },
    { path: 'AsignarMembresia', component: AsignarMembresiaComponent },
    { path: 'RegistrarAsistencia', component: AsistenciaComponent },
    { path: 'AsignarEntrenadores', component: AsignarEntrenadoresComponent },
    { path: 'RealizarPagos', component: PagosComponent },
    { path: 'HistorialPagos', component: HistorialDePagosComponent },


  ]


}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LayoutRoutingModule { }
