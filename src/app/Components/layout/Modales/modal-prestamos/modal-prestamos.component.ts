import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
@Component({
  selector: 'app-modal-prestamos',
  templateUrl: './modal-prestamos.component.html',
  styleUrl: './modal-prestamos.component.css'
})
export class ModalPrestamosComponent {

  comentariosSeparados: string[];

  constructor(@Inject(MAT_DIALOG_DATA) public data: any) {
    // Procesar los comentarios desde data.comentarios
    const comentarios: string = data.comentarios || ''; // Asegúrate de que no sea null
    // Dividir en función de un salto de línea o cualquier otro separador que utilices
    this.comentariosSeparados = comentarios.split('\n').map(item => item.trim()).filter(item => item.length > 0);
  }

  // Este método solo retorna el comentario tal como está, sin agregar paréntesis
  formatComentario(comentario: string): string {
    return comentario; // Solo devolver el comentario tal cual
  }


}
