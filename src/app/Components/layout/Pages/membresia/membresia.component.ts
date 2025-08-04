import { Component, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Membresia } from '../../../../Interfaces/membresia';
import { MembresiaService } from '../../../../Services/membresia.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { UsuariosService } from '../../../../Services/usuarios.service';
import { UtilidadService } from '../../../../Reutilizable/utilidad.service';
import Swal from 'sweetalert2';
import { ModalMembresiaComponent } from '../../Modales/modal-membresia/modal-membresia.component';

@Component({
  selector: 'app-membresia',
  templateUrl: './membresia.component.html',
  styleUrl: './membresia.component.css'
})
export class MembresiaComponent {


  columnasTabla: string[] = ['nombre','descripcion', 'duracionDias','precio' ,'estado', 'acciones'];
  dataInicio: Membresia[] = [];
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



   obtenerMembresia() {

    this._mebresiaServicio.listarMembresias().subscribe({

      next: (data) => {
        if (data.status) {

          data.value.sort((a: Membresia, b: Membresia) => a.nombre.localeCompare(b.nombre));
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
                  this.obtenerMembresia();
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
    this.obtenerMembresia();

  }


  ngAfterViewInit(): void {
    this.dataListaMembresia.paginator = this.paginacionTabla;
  }

  aplicarFiltroTabla(event: Event) {
    const filtreValue = (event.target as HTMLInputElement).value;
    this.dataListaMembresia.filter = filtreValue.trim().toLocaleLowerCase();
  }



  nuevaMembresia() {

    this.dialog.open(ModalMembresiaComponent, {
      disableClose: true

    }).afterClosed().subscribe(resultado => {

      if (resultado === "true") this.obtenerMembresia();

    });

  }

  editarProducto(membresia: Membresia) {

    this.dialog.open(ModalMembresiaComponent, {
      disableClose: true,
      data: membresia
    }).afterClosed().subscribe(resultado => {

      if (resultado === "true") this.obtenerMembresia();

    });
  }


  formatearNumero(numero: string): string {
    // Convierte la cadena a número
    const valorNumerico = parseFloat(numero.replace(',', '.'));

    // Verifica si es un número válido
    if (!isNaN(valorNumerico)) {
      // Formatea el número con comas como separadores de miles y dos dígitos decimales
      return valorNumerico.toLocaleString('es-CO', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    } else {
      // Devuelve la cadena original si no se puede convertir a número
      return numero;
    }
  }

  eliminarProducto(membresia: Membresia) {

    Swal.fire({

      title: "¿Desea eliminar la acategoria?",
      text: membresia.nombre,
      icon: "warning",
      confirmButtonColor: '#3085d6',
      confirmButtonText: "Si, eliminar",
      showCancelButton: true,
      cancelButtonColor: '#d33',
      cancelButtonText: 'No, volver'

    }).then((resultado) => {


      if (resultado.isConfirmed) {

        this._mebresiaServicio.eliminarMembresia(membresia.idMembresia!).subscribe({
          next: (data) => {

            if (data.status) {
              Swal.fire({
                icon: 'success',
                title: 'Membresia Eliminada',
                text: `La membresia fue eliminada`,
              });
              // this._utilidadServicio.mostrarAlerta("La categoria fue eliminado","listo!");
              this.obtenerMembresia();
            } else {
              Swal.fire({
                icon: 'error',
                title: 'ERROR',
                text: `No se pudo eliminar la membresia`,
              });
              // this._utilidadServicio.mostrarAlerta("No se pudo eliminar la categoria","Error");

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
                      this.eliminar(membresia);
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
  eliminar(membresia: Membresia) {
    this._mebresiaServicio.eliminarMembresia(membresia.idMembresia!).subscribe({
      next: (data) => {

        if (data.status) {
          Swal.fire({
            icon: 'success',
            title: 'Categoria Eliminada',
            text: `La categoria fue eliminada`,
          });
          // this._utilidadServicio.mostrarAlerta("La categoria fue eliminado","listo!");
          this.obtenerMembresia();
        } else {
          Swal.fire({
            icon: 'error',
            title: 'ERROR',
            text: `No se pudo eliminar la categoria`,
          });
          // this._utilidadServicio.mostrarAlerta("No se pudo eliminar la categoria","Error");

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
                  this.eliminar(membresia);
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




}
