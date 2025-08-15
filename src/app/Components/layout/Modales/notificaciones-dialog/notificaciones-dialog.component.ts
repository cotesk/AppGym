import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { AsignacionMembresia } from '../../../../Interfaces/asignacionMembresia';
import { MembresiaService } from '../../../../Services/membresia.service';

@Component({
  selector: 'app-notificaciones-dialog',
  templateUrl: './notificaciones-dialog.component.html',
  styleUrl: './notificaciones-dialog.component.css'
})
export class NotificacionesDialogComponent {


  fechas: AsignacionMembresia[];
  paginaActual = 1;
  fechasPorPagina = 5;

  constructor(
    public dialogRef: MatDialogRef<NotificacionesDialogComponent>,
    private dialog: MatDialog,
    private fechaservice: MembresiaService,

    @Inject(MAT_DIALOG_DATA) public data: { fechas: AsignacionMembresia[] }
  ) {
    this.fechas = data.fechas;
    // console.log('datos ', this.data);
  }

  // Método para cerrar el diálogo y confirmar la notificación
  cerrarDialog(confirmado: boolean): void {
    this.dialogRef.close(confirmado);
  }


  // Método para cambiar de página
  cambiarPagina(pagina: number) {
    this.paginaActual = pagina;
  }

  // Método para calcular el índice del primer elemento en la página actual
  indiceInicial() {
    return (this.paginaActual - 1) * this.fechasPorPagina;
  }

  // Método para calcular el índice del último elemento en la página actual
  indiceFinal() {
    return Math.min(this.indiceInicial() + this.fechasPorPagina - 1, this.fechas.length - 1);
  }


  // Método para generar un array de números que representan las páginas disponibles
  paginas() {
    const totalPaginas = Math.ceil(this.fechas.length / this.fechasPorPagina);
    return Array(totalPaginas).fill(0).map((x, i) => i + 1);
  }

  // Método para obtener los fechas de la página actual
  fechasDePagina() {
    const inicio = (this.paginaActual - 1) * this.fechasPorPagina;
    return this.fechas.slice(inicio, inicio + this.fechasPorPagina);
  }

abrirWhatsapp(numero: string, nombre?: string) {
  if (!numero) return;
  const mensaje = `Hola ${nombre || ''}, tu membresía del gym ha vencido. Te invitamos a renovarla para seguir entrenando con nosotros. 💪🏽`;
  const url = `https://wa.me/57${numero}?text=${encodeURIComponent(mensaje)}`;
  window.open(url, '_blank');
}



}
