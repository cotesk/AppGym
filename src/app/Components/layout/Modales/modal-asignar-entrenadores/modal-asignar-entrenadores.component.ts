import { ChangeDetectorRef, Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { environment } from '../../../../../environments/environment';
import { Usuario } from '../../../../Interfaces/usuario';
import * as CryptoJS from 'crypto-js';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { UsuariosService } from '../../../../Services/usuarios.service';
import { UtilidadService } from '../../../../Reutilizable/utilidad.service';
import { EntrenadorClienteService } from '../../../../Services/entrenador.service';
import { DomSanitizer } from '@angular/platform-browser';
import { EntrenadorCliente } from '../../../../Interfaces/entrenadorCliente';
import { VerImagenProductoModalComponent } from '../ver-imagen-producto-modal/ver-imagen-producto-modal.component';
import { AsignacionEntrenadorCliente } from '../../../../Interfaces/asignacionEntrenadorCliente';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-modal-asignar-entrenadores',
  templateUrl: './modal-asignar-entrenadores.component.html',
  styleUrl: './modal-asignar-entrenadores.component.css'
})
export class ModalAsignarEntrenadoresComponent {


  formularioAsignar: FormGroup;

  tituloAccion: string = "Agregar";
  botonAccion: string = "Guardar";

  listaEntrenador: Usuario[] = [];
  listaCliente: Usuario[] = [];
  imagenSeleccionada: string | null = null;
  imagenSeleccionadaEntrenador: string | null = null;
  listaClientesFiltrada: Usuario[] = [];
  urlApi: string = environment.endpoint;
  // membresiaSeleccionado!: Membresia | null;

  numeroFormateado: string = '';

  public loading: boolean = false;
  imagenes: any[] = [];
  imagenBlob: Blob = new Blob();
  modoEdicion: boolean = false;

  private readonly CLAVE_SECRETA = '9P#5a^6s@Lb!DfG2@17#Co-Tes#07';

  nuevoArchivo: File | null = null;
  tipodeFijo: string = "Si";
  precioCompra: number = 0;
  categoriaSeleccionada: number | null = null;
  nombreUsuario: string = '';
  rolUsuario: string = '';

  clienteFiltrado: string = '';
  // Asegúrate de inicializar esta lista
  constructor(
    private modalActual: MatDialogRef<ModalAsignarEntrenadoresComponent>,
    @Inject(MAT_DIALOG_DATA) public datosMembresia: EntrenadorCliente, private fb: FormBuilder,
    private _entrenadorServicio: EntrenadorClienteService,
    private _utilidadServicio: UtilidadService, private sanitizer: DomSanitizer,
    private cdRef: ChangeDetectorRef,
    private _usuarioServicio: UsuariosService,
    private dialog: MatDialog,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {



    this.formularioAsignar = this.fb.group({

      clienteId: ['', [Validators.required]],
      entrenadorId: ['', Validators.required],




    });

    if (datosMembresia != null) {
      this.tituloAccion = "Editar";
      this.botonAccion = "Actualizar";
      this.modoEdicion = true;
    }



    this._usuarioServicio.listaUsuario().subscribe({

      next: (data) => {
        if (data.status) {

          this.listaEntrenador = data.value.sort((a: Usuario, b: Usuario) => a.nombreCompleto!.localeCompare(b.nombreCompleto!));

          const lista = data.value as Usuario[];
          this.listaEntrenador = lista.filter(p => p.esActivo == 1 &&
            p.rolDescripcion != "Administrador" && p.rolDescripcion !== "Clientes")



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

    this._usuarioServicio.listaUsuario().subscribe({

      next: (data) => {
        if (data.status) {

          // this.listaCliente = data.value.sort((a: Usuario, b: Usuario) => a.nombreCompleto!.localeCompare(b.nombreCompleto!));

          // const lista = data.value as Usuario[];
          // this.listaCliente = lista;

             this.listaCliente = data.value.sort((a: Usuario, b: Usuario) => a.nombreCompleto!.localeCompare(b.nombreCompleto!));

          const lista = data.value as Usuario[];
          this.listaCliente = lista.filter(p => p.esActivo == 1 && p.rolDescripcion == "Clientes")

            // console.log(this.listaCliente);

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




  mostrarListaClientes(): void {
    this.listaClientesFiltrada = this.listaCliente;
  }

  mostrarUsuario(usuario: Usuario): string {

    return usuario.nombreCompleto!;

  } lastItem(item: any, list: any[]): boolean {
    return item === list[list.length - 1];
  }


  ngOnInit(): void {
    if (this.datosMembresia != null) {

      this.formularioAsignar.patchValue({
        entrenadorId: this.datosMembresia.entrenadorId,
        clienteId: this.datosMembresia.clienteId,

      });

      // Primero cargamos la lista de usuarios
      this._usuarioServicio.listaUsuario().subscribe({
        next: (data) => {
          if (data.status) {
            // Filtrar los usuarios según las condiciones
            const lista = data.value as Usuario[];

            this.listaEntrenador = lista
              .filter(p => p.esActivo == 1 && p.rolDescripcion !== "Administrador" && p.rolDescripcion !== "Clientes")
              .sort((a, b) => a.nombreCompleto!.localeCompare(b.nombreCompleto!)); // Ordenar por nombre

            // Ahora buscamos el usuario con la idUsuario de datosMembresia
            if (this.datosMembresia.entrenadorId) {
              const usuario = this.listaEntrenador.find(item => item.idUsuario === this.datosMembresia.entrenadorId);
              this.nombreUsuario = usuario?.nombreCompleto!;
              this.rolUsuario = usuario?.rolDescripcion!;
              this.cargarImagenEntrenador(this.datosMembresia.entrenadorId); // Cargar imagen después
            }
          }
        },
        error: (error) => {
          console.error('Error al cargar la lista de usuarios:', error);
        }
      });


      // Primero cargamos la lista de usuarios
      this._usuarioServicio.listaClientes().subscribe({
        next: (data) => {
          if (data.status) {
            // Filtrar los usuarios según las condiciones
            const lista = data.value as Usuario[];

            this.listaCliente = lista
              .filter(p => p.esActivo == 1)
              .sort((a, b) => a.nombreCompleto!.localeCompare(b.nombreCompleto!)); // Ordenar por nombre

            // Ahora buscamos el usuario con la idUsuario de datosMembresia
            if (this.datosMembresia.clienteId) {
              const usuario = this.listaCliente.find(item => item.idUsuario === this.datosMembresia.clienteId);
              this.nombreUsuario = usuario?.nombreCompleto!;
              this.rolUsuario = usuario?.rolDescripcion!;
              this.cargarImagenProducto(this.datosMembresia.clienteId); // Cargar imagen después
            }
          }
        },
        error: (error) => {
          console.error('Error al cargar la lista de usuarios:', error);
        }
      });





    }





  }




  usuario() {

    this._usuarioServicio.listaUsuario().subscribe({

      next: (data) => {
        if (data.status) {

          this.listaEntrenador = data.value.sort((a: Usuario, b: Usuario) => a.nombreCompleto!.localeCompare(b.nombreCompleto!));

          const lista = data.value as Usuario[];
          this.listaEntrenador = lista.filter(p => p.esActivo == 1 &&
            p.rolDescripcion != "Administrador" && p.rolDescripcion !== "Clientes")

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


  formatearNumero(event: any, campo: string): void {
    let valorInput = event.target.value.replace(/\./g, ''); // Elimina los puntos existentes

    // Verifica si el valor es un número válido antes de formatear
    if (valorInput !== '' && !isNaN(parseFloat(valorInput))) {
      valorInput = parseFloat(valorInput).toLocaleString('es-CO', { maximumFractionDigits: 2 });
      this.numeroFormateado = valorInput;

      // Actualiza el valor formateado en el formulario
      this.formularioAsignar.get(campo)?.setValue(valorInput);
    } else {
      // Si el valor no es un número válido o está vacío, establece el valor en blanco en el formulario
      this.numeroFormateado = '';
      this.formularioAsignar.get(campo)?.setValue('');
    }
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




  private cargarImagenProducto(idUsuario: number) {
     console.log(idUsuario);
    this._usuarioServicio.obtenerImagenUsuario(idUsuario).subscribe(
      (response: any) => {
        if (response && response.imagenUrl) {
          this.imagenSeleccionada = `${response.imagenUrl}`;
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
  private cargarImagenEntrenador(idUsuario: number) {
    this._usuarioServicio.obtenerImagenUsuario(idUsuario).subscribe(
      (response: any) => {
        if (response && response.imagenUrl) {
          this.imagenSeleccionadaEntrenador = `${response.imagenUrl}`;
        } else {
          console.error('Imagen no disponible');
          this.imagenSeleccionadaEntrenador = 'ruta/de/imagen/predeterminada.png'; // O deja nulo
        }
      },
      (error: any) => {
        console.error('Error al cargar la imagen:', error);
        this.imagenSeleccionadaEntrenador = 'ruta/de/imagen/predeterminada.png'; // Imagen predeterminada si falla
      }
    );
  }
  verImagen(imageData: string) {
    this.dialog.open(VerImagenProductoModalComponent, {
      data: {
        imagenes: [imageData]
      }
    });
  }

  obtenerImag(event: any) {

    // Obtener el ID seleccionado
    const idMembresiaSeleccionada = event.option.value;
   console.log(idMembresiaSeleccionada);
    // Buscar la membresía completa en la lista
    const membresiaSeleccionada = this.listaCliente.find(item => item.idUsuario === idMembresiaSeleccionada);



    this.nombreUsuario = membresiaSeleccionada?.nombreCompleto!;
    this.rolUsuario = membresiaSeleccionada?.rolDescripcion!;

    this.cargarImagenProducto(membresiaSeleccionada?.idUsuario!);

  }

  obtenerImagEntrenador(event: any) {

    // Obtener el ID seleccionado
    const idMembresiaSeleccionada = event.value;

    // Buscar la membresía completa en la lista
    const membresiaSeleccionada = this.listaEntrenador.find(item => item.idUsuario === idMembresiaSeleccionada);



    this.nombreUsuario = membresiaSeleccionada?.nombreCompleto!;
    this.rolUsuario = membresiaSeleccionada?.rolDescripcion!;

    this.cargarImagenEntrenador(membresiaSeleccionada?.idUsuario!);

  }

  guardarEditar_Producto() {

    console.log("Estado del formulario:", this.formularioAsignar.status);

    // Verificar si el formulario es inválido
    if (this.formularioAsignar.invalid) {
      this._utilidadServicio.mostrarAlerta("Por favor, complete todos los campos correctamente", "Error");
      return;
    }



    const _membresia: AsignacionEntrenadorCliente = {


      clienteID: this.formularioAsignar.value.clienteId,
      entrenadorID: this.formularioAsignar.value.entrenadorId,



    };

    if (this.datosMembresia == null) {
      this._entrenadorServicio.asignarEntrenadorACliente(_membresia).subscribe({
        next: (data) => {
          if (data.mensaje == "Entrenador asignado exitosamente") {
            Swal.fire({
              icon: 'success',
              title: 'Entrenador Registrado',
              text: `El entrenador fue asignado`,
            });
            // this._utilidadServicio.mostrarAlerta("El producto fue registrado", "Exito");
            this.modalActual.close("true");
          } else {
            Swal.fire({
              icon: 'error',
              title: 'ERROR',
              text: `No se pudo guardar la membresia`,
            });

          }

          // this._utilidadServicio.mostrarAlerta("Ya existe un producto con ese mismo nombre", "Error");

        },
        error: (error) => {

          if (error.error.mensaje == "El cliente ya tiene un entrenador asignado.") {
            Swal.fire({
              icon: 'error',
              title: 'ERROR',
              text: `El cliente ya tiene un entrenador asignado.`,
            });
            return
          } else {
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

        }





      });
    } else {
      this._entrenadorServicio.editarAsignacion(this.datosMembresia.asignacionId, _membresia).subscribe({
        next: (data) => {
          if (data.mensaje == "Asignación actualizada exitosamente.") {
            Swal.fire({
              icon: 'success',
              title: 'Asignacion Editada',
              text: `Asignación actualizada exitosamente.`,
            });
            // this._utilidadServicio.mostrarAlerta("El producto fue registrado", "Exito");
            this.modalActual.close("true");
          }

          else {
            Swal.fire({
              icon: 'error',
              title: 'ERROR',
              text: `No se pudo editar el producto`,
            });

          }

          // this._utilidadServicio.mostrarAlerta("Ya existe un producto con ese mismo nombre", "Error");

        },
        error: (e) => {

          if (e.error.mensaje == "El cliente ya está asignado a otro entrenador.") {
            Swal.fire({
              icon: 'error',
              title: 'ERROR',
              text: `El cliente ya está asignado a otro entrenador.`,
            });
            return
          } else
            if (e.error.mensaje == "Ya existe una asignación con este cliente y entrenador.") {
              Swal.fire({
                icon: 'error',
                title: 'ERROR',
                text: `Ya existe una asignación con este cliente y entrenador.`,
              });
              return
            }
            else if (e.error.mensaje == "Asignación no encontrada.") {
              Swal.fire({
                icon: 'error',
                title: 'ERROR',
                text: `Asignación no encontrada.`,
              });

              return
            }
            else {
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

        }


      });


    }
    // }


  }




}
