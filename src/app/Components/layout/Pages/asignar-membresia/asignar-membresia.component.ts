import { Component, ViewChild } from '@angular/core';
import Swal from 'sweetalert2';
import { AsignacionMembresia } from '../../../../Interfaces/asignacionMembresia';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MembresiaService } from '../../../../Services/membresia.service';
import { UsuariosService } from '../../../../Services/usuarios.service';
import { UtilidadService } from '../../../../Reutilizable/utilidad.service';
import { ModalAsignarMembresiaComponent } from '../../Modales/modal-asignar-membresia/modal-asignar-membresia.component';
import * as CryptoJS from 'crypto-js';
import { VerImagenProductoModalComponent } from '../../Modales/ver-imagen-producto-modal/ver-imagen-producto-modal.component';


@Component({
  selector: 'app-asignar-membresia',
  templateUrl: './asignar-membresia.component.html',
  styleUrl: './asignar-membresia.component.css'
})
export class AsignarMembresiaComponent {


  columnasTabla: string[] = ['imagen', 'nombreUsuario', 'nombreMembresia', 'fechaVencimiento', 'estado', 'acciones'];
  dataInicio: AsignacionMembresia[] = [];
  dataListaMembresia = new MatTableDataSource(this.dataInicio);
  @ViewChild(MatPaginator) paginacionTabla!: MatPaginator;
  private readonly CLAVE_SECRETA = '9P#5a^6s@Lb!DfG2@17#Co-Tes#07';
  @ViewChild(MatSort) sort!: MatSort;
  selectedFile: File | null = null;





  constructor(
    private dialog: MatDialog,
    private _mebresiaServicio: MembresiaService,
    private _utilidadServicio: UtilidadService,
    private _usuarioServicio: UsuariosService,

  ) { }



  obtenerAsignaciones() {

    this._mebresiaServicio.listarAsignaciones().subscribe({

      next: (data) => {
        if (data.status) {

          // data.value.sort((a: AsignacionMembresia, b: AsignacionMembresia) => a.nombreUsuario!.localeCompare(b.nombreUsuario!));
          this.dataListaMembresia.data = data.value;


        } else {

          Swal.fire({
            icon: 'warning',
            title: 'Advertencia',
            text: `no se encontraron datos`,
          });
          // this._utilidadServicio.mostrarAlerta("no se encontraron datos", "Oops!");

        }

      },
      error: (e) => {

        let idUsuario: number = 0;


        // Obtener el idUsuario del localStorage
        const usuarioString = localStorage.getItem('usuario');
        const bytes = CryptoJS.AES.decrypt(usuarioString!, this.CLAVE_SECRETA);
        const datosDesencriptados = bytes.toString(CryptoJS.enc.Utf8);
        if (datosDesencriptados !== null) {
          const usuario = JSON.parse(datosDesencriptados);
          idUsuario = usuario.idUsuario; // Obtener el idUsuario del objeto usuario

          this._usuarioServicio.obtenerUsuarioPorId(idUsuario).subscribe(
            (usuario: any) => {

              console.log('Usuario obtenido:', usuario);
              let refreshToken = usuario.refreshToken

              // Manejar la renovación del token
              this._usuarioServicio.renovarToken(refreshToken).subscribe(
                (response: any) => {
                  console.log('Token actualizado:', response.token);
                  // Guardar el nuevo token de acceso en el almacenamiento local
                  localStorage.setItem('authToken', response.token);
                  this.obtenerAsignaciones();
                },
                (error: any) => {
                  console.error('Error al actualizar el token:', error);
                }
              );



            },
            (error: any) => {
              console.error('Error al obtener el usuario:', error);
            }
          );
        }





      }

    })
  }


  ngOnInit(): void {
    this.obtenerAsignaciones();

  }

  verImagen(usuario: any): void {
    // console.log(usuario);
    this.dialog.open(VerImagenProductoModalComponent, {
      data: {
        imagenes: [usuario.imagenUrl]
      }
    });
  }

  ngAfterViewInit(): void {
    this.dataListaMembresia.paginator = this.paginacionTabla;
  }

  aplicarFiltroTabla(event: Event) {
    const filtreValue = (event.target as HTMLInputElement).value;
    this.dataListaMembresia.filter = filtreValue.trim().toLocaleLowerCase();
  }

  editarAsignacion(asignar: AsignacionMembresia) {

    this.dialog.open(ModalAsignarMembresiaComponent, {
      disableClose: true,
      data: asignar
    }).afterClosed().subscribe(resultado => {

      if (resultado === "true") this.obtenerAsignaciones();

    });



  }

  nuevaMembresia() {

    this.dialog.open(ModalAsignarMembresiaComponent, {
      disableClose: true

    }).afterClosed().subscribe(resultado => {

      if (resultado === "true") this.obtenerAsignaciones();

    });

  }

}
