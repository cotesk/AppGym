import { Component, ViewChild } from '@angular/core';
import { EntrenadorCliente } from '../../../../Interfaces/entrenadorCliente';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { EntrenadorClienteService } from '../../../../Services/entrenador.service';
import { UtilidadService } from '../../../../Reutilizable/utilidad.service';
import { UsuariosService } from '../../../../Services/usuarios.service';
import Swal from 'sweetalert2';
import * as CryptoJS from 'crypto-js';
import { ModalAsignarEntrenadoresComponent } from '../../Modales/modal-asignar-entrenadores/modal-asignar-entrenadores.component';


@Component({
  selector: 'app-asignar-entrenadores',
  templateUrl: './asignar-entrenadores.component.html',
  styleUrl: './asignar-entrenadores.component.css'
})
export class AsignarEntrenadoresComponent {



  columnasTabla: string[] = ['asignacionId','nombreEntrenador', 'nombreCliente','fechaAsignacion', 'acciones' ];
  dataInicio: EntrenadorCliente[] = [];
  dataListaMembresia = new MatTableDataSource(this.dataInicio);
  @ViewChild(MatPaginator) paginacionTabla!: MatPaginator;
  private readonly CLAVE_SECRETA = '9P#5a^6s@Lb!DfG2@17#Co-Tes#07';
  @ViewChild(MatSort) sort!: MatSort;
  selectedFile: File | null = null;





  constructor(
    private dialog: MatDialog,
    private _entrenadorServicio: EntrenadorClienteService,
    private _utilidadServicio: UtilidadService,
    private _usuarioServicio: UsuariosService,

  ) { }



   obtenerEntrenadores() {

    this._entrenadorServicio.getTodasLasAsignaciones().subscribe({

      next: (data) => {
        if (data?.length > 0) {

          data.sort((a: EntrenadorCliente, b: EntrenadorCliente) => a.nombreEntrenador!.localeCompare(b.nombreEntrenador!));
          this.dataListaMembresia.data = data;

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
                  this.obtenerEntrenadores();
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
    this.obtenerEntrenadores();

  }


  eliminar(entrenador:EntrenadorCliente){

    Swal.fire({

      title: "¿Desea eliminar con este entrenador?",
      text: entrenador.nombreEntrenador,
      icon: "warning",
      confirmButtonColor: '#3085d6',
      confirmButtonText: "Si, eliminar",
      showCancelButton: true,
      cancelButtonColor: '#d33',
      cancelButtonText: 'No, volver'

    }).then((resultado) => {


      if (resultado.isConfirmed) {
        this._entrenadorServicio.eliminarAsignacion(entrenador.asignacionId!).subscribe({
          next: (data) => {

            if (data.mensaje =="Asignación eliminada exitosamente.") {
              Swal.fire({
                icon: 'success',
                title: 'Usuario Eliminado',
                text: `El usuario fue eliminado`,
              })
              // this._utilidadServicio.mostrarAlerta("El usuario fue eliminado", "listo!");
              this.obtenerEntrenadores();
            } else {
              Swal.fire({
                icon: 'error',
                title: 'ERROR',
                text: `No se pudo eliminar el usuario`,
              });
              // this._utilidadServicio.mostrarAlerta("No se pudo eliminar el usuario", "Error");
              this.obtenerEntrenadores();
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
                      this.obtenerEntrenadores();
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


    })

  }

  ngAfterViewInit(): void {
    this.dataListaMembresia.paginator = this.paginacionTabla;
  }

  aplicarFiltroTabla(event: Event) {
    const filtreValue = (event.target as HTMLInputElement).value;
    this.dataListaMembresia.filter = filtreValue.trim().toLocaleLowerCase();
  }

  editarAsignacion(asignar: EntrenadorCliente){

    this.dialog.open(ModalAsignarEntrenadoresComponent, {
      disableClose: true,
      data: asignar
    }).afterClosed().subscribe(resultado => {

      if (resultado === "true") this.obtenerEntrenadores();

    });



  }

  nuevoEntrenador() {

    this.dialog.open(ModalAsignarEntrenadoresComponent, {
      disableClose: true

    }).afterClosed().subscribe(resultado => {

      if (resultado === "true") this.obtenerEntrenadores();

    });

  }



}
