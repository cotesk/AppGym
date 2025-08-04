import { ChangeDetectorRef, Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import Swal from 'sweetalert2';
import * as CryptoJS from 'crypto-js';
import { AsitenciaPersonal } from '../../../../Interfaces/asistenciaPersonal';
import { Usuario } from '../../../../Interfaces/usuario';
import { environment } from '../../../../../environments/environment';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { AsistenciaPersonalService } from '../../../../Services/asistencia.service';
import { UtilidadService } from '../../../../Reutilizable/utilidad.service';
import { UsuariosService } from '../../../../Services/usuarios.service';
import { DomSanitizer } from '@angular/platform-browser';
import { VerImagenProductoModalComponent } from '../ver-imagen-producto-modal/ver-imagen-producto-modal.component';

@Component({
  selector: 'app-modal-asistencia',
  templateUrl: './modal-asistencia.component.html',
  styleUrl: './modal-asistencia.component.css'
})
export class ModalAsistenciaComponent {

  listaClientes: Usuario[] = [];
  listaClientesFiltrada: Usuario[] = [];
  clienteSeleccionado!: any | null;
  idUsuarioSeleccionado!: any | null;

  formularioAsistencia: FormGroup;

  tituloAccion: string = "Agregar";
  botonAccion: string = "Guardar";

  listaUsuario: Usuario[] = [];
  imagenSeleccionada: string | null = null;

  nombreUsuario: string = '';
  rolUsuario: string = '';


  public loading: boolean = false;
  imagenes: any[] = [];
  imagenBlob: Blob = new Blob();
  modoEdicion: boolean = false;

  private readonly CLAVE_SECRETA = '9P#5a^6s@Lb!DfG2@17#Co-Tes#07';

  nuevoArchivo: File | null = null;
  tipodeFijo: string = "Si";
  precioCompra: number = 0;
  categoriaSeleccionada: number | null = null;

  clienteFiltrado: string = '';
  // Asegúrate de inicializar esta lista
  constructor(
    private modalActual: MatDialogRef<ModalAsistenciaComponent>,
    @Inject(MAT_DIALOG_DATA) public datosMembresia: AsitenciaPersonal, private fb: FormBuilder,
    private _asistenciaServicio: AsistenciaPersonalService,
    private _utilidadServicio: UtilidadService, private sanitizer: DomSanitizer,
    private cdRef: ChangeDetectorRef,
    private _usuarioServicio: UsuariosService,
    private dialog: MatDialog,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {



    this.formularioAsistencia = this.fb.group({

      usuario: ['', []],
    });

    if (datosMembresia != null) {
      this.tituloAccion = "Editar";
      this.botonAccion = "Actualizar";
      this.modoEdicion = true;
    }



    this._usuarioServicio.listaClientes().subscribe({

      next: (data) => {
        if (data.status) {

          this.listaClientes = data.value.sort((a: Usuario, b: Usuario) => a.nombreCompleto!.localeCompare(b.nombreCompleto!));

          const lista = data.value as Usuario[];
          this.listaClientes = lista.filter(p => p.esActivo == 1)


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
                  this.usuario();
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

    this.formularioAsistencia.get('usuario')?.valueChanges.subscribe(value => {
      this.listaClientesFiltrada = this.retornarUsuariosPorFiltro(value);
    });

  }



  ngOnInit(): void {

  this.usuario();

  this.formularioAsistencia = this.fb.group({

    usuario: ['', []],
  });


  this.formularioAsistencia.get('usuario')?.valueChanges.subscribe(value => {
    this.listaClientesFiltrada = this.retornarUsuariosPorFiltro(value);
  });

  }

  retornarUsuariosPorFiltro(busqueda: any): Usuario[] {
    const valorBuscado = typeof busqueda === "string" ? busqueda.toLocaleLowerCase() : busqueda.nombreCompleto.toLocaleLowerCase()
    return this.listaClientes.filter(item => item.nombreCompleto!.toLocaleLowerCase().includes(valorBuscado));

  }

  usuario() {

    this._usuarioServicio.listaClientes().subscribe({

      next: (data) => {
        if (data.status) {

          this.listaUsuario = data.value.sort((a: Usuario, b: Usuario) => a.nombreCompleto!.localeCompare(b.nombreCompleto!));

          const lista = data.value as Usuario[];
          this.listaUsuario = lista.filter(p => p.esActivo == 1)


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
                  this.usuario();
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

  private cargarImagenProducto(idUsuario: number) {
    this._usuarioServicio.obtenerImagenUsuario(idUsuario).subscribe(
      (response: any) => {
        if (response && response.imageData) {
          this.imagenSeleccionada = `data:image/png;base64,${response.imageData}`;
        } else {
          console.error('Imagen no disponible');
          this.imagenSeleccionada = 'ruta/de/imagen/predeterminada.png'; // O deja nulo
        }
      },
      (error: any) => {
        console.error('Error al cargar la imagen:', error);
        this.imagenSeleccionada = 'ruta/de/imagen/predeterminada.png'; // Imagen predeterminada si falla
      }
    );
  }
  verImagen(imageData: string) {
    this.dialog.open(VerImagenProductoModalComponent, {
      data: {
        imageData: imageData
      }
    });
  }


  mostrarUsuario(usuario: Usuario): string {

    return usuario.nombreCompleto!;

  }

  mostrarListaClientes(): void {
    this.listaClientesFiltrada = this.listaClientes;
  }

  lastItem(item: any, list: any[]): boolean {
    return item === list[list.length - 1];
  }


  obtenerImag(event: any) {

    const selectedProduct: Usuario = event.option.value;
    // Obtener el ID seleccionado
    const idMembresiaSeleccionada = event.option.value;
    this.clienteSeleccionado = idMembresiaSeleccionada
    // Buscar la membresía completa en la lista
    const membresiaSeleccionada = this.listaUsuario.find(item => item.idUsuario === idMembresiaSeleccionada);

    //me deja el input vacio una vez seleccione un nombre
    this.formularioAsistencia.get('usuario')?.setValue(selectedProduct.nombreCompleto);

    this.nombreUsuario = membresiaSeleccionada?.nombreCompleto!;
    this.rolUsuario = membresiaSeleccionada?.rolDescripcion!;

    this.cargarImagenProducto(membresiaSeleccionada?.idUsuario!);

  }
  token() {
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
              this.guardarEditar_Producto();
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



  guardarEditar_Producto() {

    console.log("Estado del formulario:", this.formularioAsistencia.status);

    // Verificar si el formulario es inválido
    if (this.formularioAsistencia.invalid) {
      this._utilidadServicio.mostrarAlerta("Por favor, complete todos los campos correctamente", "Error");
      return;
    }

    let idUsuario: number = parseInt(this.clienteSeleccionado)


    this._asistenciaServicio.registrarAsistencia(idUsuario).subscribe({
      next: (data) => {
        if (data.status) {
          Swal.fire({
            icon: 'success',
            title: 'Asistencia Registrada',
            text: `La Asistencia fue registrada`,
          });
          // this._utilidadServicio.mostrarAlerta("El producto fue registrado", "Exito");
          this.modalActual.close("true");
        } else {

          if (data.msg == "Ya se ha registrado una asistencia para el día de hoy.") {
            Swal.fire({
              icon: 'error',
              title: 'ERROR',
              text: `Ya se ha registrado una asistencia para el día de hoy.`,
            });
            return

          } else {
            Swal.fire({
              icon: 'error',
              title: 'ERROR',
              text: `No se pudo guardar la asistencia.`,
            });
            return
          }

          // this._utilidadServicio.mostrarAlerta("Ya existe un producto con ese mismo nombre", "Error");
        }
      },
      error: (error) => {
        console.log('Error completo:', error);

        if (error.msg === "El usuario no tiene una membresía activa.") {

          Swal.fire({
            icon: 'error',
            title: 'ERROR',
            text: 'El usuario no tiene una membresía activa.',
          });
          return

        }else if (error.msg === "Ya se ha registrado una asistencia para el día de hoy.") {

          Swal.fire({
            icon: 'error',
            title: 'ERROR',
            text: 'Ya se ha registrado una asistencia para el día de hoy.',
          });
          return

        }

        else if (error.status === 401) {

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
                    this.guardarEditar_Producto();
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



        } else {


          Swal.fire({
            icon: 'error',
            title: 'ERROR',
            text: `Ya se ha registrado una asistencia para el día de hoy.`,
          });

          return

        }


      }





    });


  }



}
