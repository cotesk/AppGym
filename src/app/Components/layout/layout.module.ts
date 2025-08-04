import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { LayoutRoutingModule } from './layout-routing.module';
import { UsuarioComponent } from './Pages/usuario/usuario.component';
import { ModalUsuarioComponent } from './Modales/modal-usuario/modal-usuario.component';
import { ModalTemporizadorComponent } from './Modales/modal-temporizador/modal-temporizador.component';
import { SharedModule } from '../../Reutilizable/shared/shared.module';
import { NgxExtendedPdfViewerModule } from 'ngx-extended-pdf-viewer';
import { MatStepperModule } from '@angular/material/stepper';
import { CarouselModule } from 'ngx-bootstrap/carousel';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ImagenDialogComponent } from './Modales/imagen-dialog/imagen-dialog.component';
import { ChangeInfoModalComponent } from './Modales/change-info-modal/change-info-modal.component';
import { CambiarImagenUsuarioComponent } from './Modales/cambiar-imagen-usuario/cambiar-imagen-usuario.component';
import { ModalCambioImagenUsuarioComponent } from './Modales/modal-cambio-imagen-usuario/modal-cambio-imagen-usuario.component';
import { NotificacionesDialogComponent } from './Modales/notificaciones-dialog/notificaciones-dialog.component';
import { MenuComponent } from './Pages/menu/menu.component';

import { CajaComponent } from './Pages/caja/caja.component';
import { ModalAbrirCajaComponent } from './Modales/modal-abrir-caja/modal-abrir-caja.component';
import { ModalPrestamosComponent } from './Modales/modal-prestamos/modal-prestamos.component';
import { ModalCaracteristicasProductoComponent } from './Modales/modal-caracteristicas-producto/modal-caracteristicas-producto.component';
import { VerImagenProductoModalComponent } from './Modales/ver-imagen-producto-modal/ver-imagen-producto-modal.component';
import { ModalListaClientesComponent } from './Modales/modal-lista-clientes/modal-lista-clientes.component';
import { EmpresaComponent } from './Pages/empresa/empresa.component';
import { ModalEmpresaComponent } from './Modales/modal-empresa/modal-empresa.component';
import { CambiarImagenEmpresaComponent } from './Modales/cambiar-imagen-empresa/cambiar-imagen-empresa.component';
import { ModalContenidoComponent } from './Modales/modal-contenido/modal-contenido.component';
import { ContenidoComponent } from './Pages/contenido/contenido.component';
import { CambiarImagenContenidoComponent } from './Modales/cambiar-imagen-contenido/cambiar-imagen-contenido.component';
import { ColoresComponent } from './Pages/colores/colores.component';
import { MembresiaComponent } from './Pages/membresia/membresia.component';
import { ModalMembresiaComponent } from './Modales/modal-membresia/modal-membresia.component';
import { AsignarMembresiaComponent } from './Pages/asignar-membresia/asignar-membresia.component';
import { ModalAsignarMembresiaComponent } from './Modales/modal-asignar-membresia/modal-asignar-membresia.component';
import { ModalAsistenciaComponent } from './Modales/modal-asistencia/modal-asistencia.component';
import { AsistenciaComponent } from './Pages/asistencia/asistencia.component';
import { AsignarEntrenadoresComponent } from './Pages/asignar-entrenadores/asignar-entrenadores.component';
import { ModalAsignarEntrenadoresComponent } from './Modales/modal-asignar-entrenadores/modal-asignar-entrenadores.component';
import { DashBoardComponent } from './Pages/dashboard/dashboard.component';
import { PagosComponent } from './Pages/pagos/pagos.component';
import { HistorialDePagosComponent } from './Pages/historial-de-pagos/historial-de-pagos.component';
import { CalendarioModalComponent } from './Modales/calendario-modal/calendario-modal.component';


@NgModule({
  declarations: [
    UsuarioComponent,
    ModalUsuarioComponent,
    ModalTemporizadorComponent,
    ImagenDialogComponent,
    ChangeInfoModalComponent,
    CambiarImagenUsuarioComponent,
    ModalCambioImagenUsuarioComponent,
    NotificacionesDialogComponent,
    MenuComponent,
    CajaComponent,
    ModalAbrirCajaComponent,
    ModalPrestamosComponent,
    ModalCaracteristicasProductoComponent,
    VerImagenProductoModalComponent,
    ModalListaClientesComponent,
    EmpresaComponent,
    ModalEmpresaComponent,
    CambiarImagenEmpresaComponent,
    ModalContenidoComponent,
    ContenidoComponent,
    CambiarImagenContenidoComponent,
    ColoresComponent,
    MembresiaComponent,
    ModalMembresiaComponent,
    AsignarMembresiaComponent,
    ModalAsignarMembresiaComponent,
    ModalAsistenciaComponent,
    AsistenciaComponent,
    AsignarEntrenadoresComponent,
    ModalAsignarEntrenadoresComponent,
    DashBoardComponent,
    PagosComponent,
    HistorialDePagosComponent,
    CalendarioModalComponent
  ],
  imports: [
    CommonModule,
    LayoutRoutingModule,
    SharedModule,
    MatTooltipModule,
    CarouselModule.forRoot(),
    MatStepperModule,
    NgxExtendedPdfViewerModule,
  ],
})
export class LayoutModule { }
