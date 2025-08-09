import { ChangeDetectorRef, Component, Inject } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AsignacionMembresia } from '../../../../Interfaces/asignacionMembresia';
import { environment } from '../../../../../environments/environment';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MembresiaService } from '../../../../Services/membresia.service';
import { DomSanitizer } from '@angular/platform-browser';
import { UtilidadService } from '../../../../Reutilizable/utilidad.service';
import { UsuariosService } from '../../../../Services/usuarios.service';
import Swal from 'sweetalert2';
import * as CryptoJS from 'crypto-js';
import { Membresia } from '../../../../Interfaces/membresia';
import { Usuario } from '../../../../Interfaces/usuario';
import { VerImagenProductoModalComponent } from '../ver-imagen-producto-modal/ver-imagen-producto-modal.component';

@Component({
  selector: 'app-modal-asignar-membresia',
  templateUrl: './modal-asignar-membresia.component.html',
  styleUrl: './modal-asignar-membresia.component.css'
})
export class ModalAsignarMembresiaComponent {


  listaClientes: Usuario[] = [];
  listaClientesFiltrada: Usuario[] = [];
  clienteSeleccionado!: any | null;
  idUsuarioSeleccionado!: any | null;
  formularioMembresia: FormGroup;

  tituloAccion: string = "Agregar";
  botonAccion: string = "Guardar";
  listaMembresia: Membresia[] = [];
  listaMembresiaFiltrada: Usuario[] = [];
  listaUsuario: Usuario[] = [];
  imagenSeleccionada: string | null = null;

  urlApi: string = environment.endpoint;
  membresiaSeleccionado!: Membresia | null;

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
    private modalActual: MatDialogRef<ModalAsignarMembresiaComponent>,
    @Inject(MAT_DIALOG_DATA) public datosMembresia: AsignacionMembresia, private fb: FormBuilder,
    private _membresiaServicio: MembresiaService,
    private _utilidadServicio: UtilidadService, private sanitizer: DomSanitizer,
    private cdRef: ChangeDetectorRef,
    private _usuarioServicio: UsuariosService,
    private dialog: MatDialog,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {



    this.formularioMembresia = this.fb.group({

      idUsuario: ['', [Validators.required]],
      idMembresia: ['', Validators.required],




    });

    if (datosMembresia != null) {
      this.tituloAccion = "Editar";
      this.botonAccion = "Actualizar";
      this.modoEdicion = true;
    }



    this._membresiaServicio.listarMembresias().subscribe({

      next: (data) => {
        if (data.status) {

          this.listaMembresia = data.value.sort((a: Membresia, b: Membresia) => a.nombre.localeCompare(b.nombre));

          const lista = data.value as Membresia[];
          this.listaMembresia = lista.filter(p => p.esActivo == 1)


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
                  this.membresia();
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


    this._usuarioServicio.listaClientes().subscribe({

      next: (data) => {
        if (data.status) {

          this.listaClientes = data.value.sort((a: Usuario, b: Usuario) => a.nombreCompleto!.localeCompare(b.nombreCompleto!));

          const lista = data.value as Usuario[];
          this.listaClientes = lista.filter(p => p.esActivo == 1 )



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

    this.formularioMembresia.get('idUsuario')?.valueChanges.subscribe(value => {
      this.listaClientesFiltrada = this.retornarUsuariosPorFiltro(value);
    });



  }


  retornarUsuariosPorFiltro(busqueda: any): Usuario[] {
    const valorBuscado = typeof busqueda === "string" ? busqueda.toLocaleLowerCase() : busqueda.nombreCompleto.toLocaleLowerCase()
    return this.listaClientes.filter(item => item.nombreCompleto!.toLocaleLowerCase().includes(valorBuscado));

  }


  ngOnInit(): void {
    if (this.datosMembresia != null) {

      this.formularioMembresia.patchValue({
        idMembresia: this.datosMembresia.idMembresia,
        idUsuario: this.datosMembresia.idUsuario,
        fechaVencimiento: this.datosMembresia.fechaVencimiento,
        estado: this.datosMembresia.estado,
      });

      // Primero cargamos la lista de usuarios
      this._usuarioServicio.listaClientes().subscribe({
        next: (data) => {
          if (data.status) {
            // Filtrar los usuarios según las condiciones
            const lista = data.value as Usuario[];

            this.listaUsuario = lista
              .filter(p => p.esActivo == 1 )
              .sort((a, b) => a.nombreCompleto!.localeCompare(b.nombreCompleto!)); // Ordenar por nombre

            // Ahora buscamos el usuario con la idUsuario de datosMembresia
            if (this.datosMembresia.idUsuario) {
              const usuario = this.listaUsuario.find(item => item.idUsuario === this.datosMembresia.idUsuario);
              this.formularioMembresia.patchValue({
                idUsuario: usuario?.idUsuario || null,
              });

              this.nombreUsuario = usuario?.nombreCompleto!;
              this.rolUsuario = usuario?.rolDescripcion!;
              this.cargarImagenProducto(this.datosMembresia.idUsuario); // Cargar imagen después
            }
          }
        },
        error: (error) => {
          console.error('Error al cargar la lista de usuarios:', error);
        }
      });

      this._membresiaServicio.listarMembresias().subscribe({
        next: (data) => {
          if (data.status) {
            this.listaMembresia = data.value as Membresia[];

            // Verificar si datosMembresia contiene una membresía asignada
            if (this.datosMembresia?.idMembresia) {
              const membresiaSeleccionada = this.listaMembresia.find(
                (item) => item.idMembresia === this.datosMembresia.idMembresia
              );
              this.formularioMembresia.patchValue({
                idMembresia: membresiaSeleccionada?.idMembresia || null,
              });
            }
          }
        },
        error: (error) => {
          console.error('Error al cargar la lista de membresías:', error);
        }
      });


      this.formularioMembresia.get('idUsuario')?.valueChanges.subscribe(value => {
        this.listaClientesFiltrada = this.retornarUsuariosPorFiltro(value);
      });




    }

    this.usuario();
    this.membresia();

    this.formularioMembresia = this.fb.group({

      idUsuario: ['', [Validators.required]],
      idMembresia: ['', Validators.required],

    });




    this.formularioMembresia.get('idUsuario')?.valueChanges.subscribe(value => {
      this.listaClientesFiltrada = this.retornarUsuariosPorFiltro(value);
    });




  }



  membresia() {
    this._membresiaServicio.listarMembresias().subscribe({

      next: (data) => {
        if (data.status) {

          this.listaMembresia = data.value.sort((a: Membresia, b: Membresia) => a.nombre.localeCompare(b.nombre));

          const lista = data.value as Membresia[];
          this.listaMembresia = lista.filter(p => p.esActivo == 1)


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
                  this.membresia();
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

  usuario() {

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


  }


  formatearNumero(event: any, campo: string): void {
    let valorInput = event.target.value.replace(/\./g, ''); // Elimina los puntos existentes

    // Verifica si el valor es un número válido antes de formatear
    if (valorInput !== '' && !isNaN(parseFloat(valorInput))) {
      valorInput = parseFloat(valorInput).toLocaleString('es-CO', { maximumFractionDigits: 2 });
      this.numeroFormateado = valorInput;

      // Actualiza el valor formateado en el formulario
      this.formularioMembresia.get(campo)?.setValue(valorInput);
    } else {
      // Si el valor no es un número válido o está vacío, establece el valor en blanco en el formulario
      this.numeroFormateado = '';
      this.formularioMembresia.get(campo)?.setValue('');
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


  mostrarListaClientes(): void {
    this.listaClientesFiltrada = this.listaClientes;
  }

  lastItem(item: any, list: any[]): boolean {
    return item === list[list.length - 1];
  }


  obtenerMembresia(event: any) {
    // Obtener el ID seleccionado
    const idMembresiaSeleccionada = event.value;

    // Buscar la membresía completa en la lista
    const membresiaSeleccionada = this.listaMembresia.find(item => item.idMembresia === idMembresiaSeleccionada);

    if (membresiaSeleccionada) {
      // Extraer la duración de días
      const duracionDias = membresiaSeleccionada.duracionDias;

      console.log("Membresía seleccionada:", membresiaSeleccionada);
      console.log("Duración en días:", duracionDias);

      // Aquí puedes usar la duración de días en tu lógica
      this.membresiaSeleccionado = membresiaSeleccionada; // Guardar la membresía seleccionada en el componente
      // this._utilidadServicio.mostrarAlerta(
      //   `Se seleccionó la membresía "${membresiaSeleccionada.nombre}" con duración de ${duracionDias} días.`,
      //   "Información"
      // );
    } else {
      console.error("No se encontró la membresía seleccionada.");
    }
  }


  private cargarImagenProducto(idUsuario: number) {
      console.log(idUsuario);
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



  // obtenerImag(event: any) {

  //   // Obtener el ID seleccionado
  //   const idMembresiaSeleccionada = event.value;

  //   // Buscar la membresía completa en la lista
  //   const membresiaSeleccionada = this.listaUsuario.find(item => item.idUsuario === idMembresiaSeleccionada);



  //   this.nombreUsuario = membresiaSeleccionada?.nombreCompleto!;
  //   this.rolUsuario = membresiaSeleccionada?.rolDescripcion!;

  //   this.cargarImagenProducto(membresiaSeleccionada?.idUsuario!);

  // }
  obtenerImag(event: any) {

    const selectedProduct: Usuario = event.option.value;
    // Obtener el ID seleccionado
    const idMembresiaSeleccionada = event.option.value;
    this.idUsuarioSeleccionado = idMembresiaSeleccionada
    // Buscar la membresía completa en la lista
    const membresiaSeleccionada = this.listaClientes.find(item => item.idUsuario === idMembresiaSeleccionada);

    //me deja el input vacio una vez seleccione un nombre
    // this.formularioMembresia.get('idUsuario')?.setValue(selectedProduct.nombreCompleto);

    this.nombreUsuario = membresiaSeleccionada?.nombreCompleto!;
    this.rolUsuario = membresiaSeleccionada?.rolDescripcion!;

    this.cargarImagenProducto(membresiaSeleccionada?.idUsuario!);

  }


  guardarEditar_Producto() {

    console.log("Estado del formulario:", this.formularioMembresia.status);

    // Verificar si el formulario es inválido
    if (this.formularioMembresia.invalid) {
      this._utilidadServicio.mostrarAlerta("Por favor, complete todos los campos correctamente", "Error");
      return;
    }


    // Obtener la fecha actual en Bogotá
    const obtenerFechaActualBogota = (): Date => {
      const fechaActual = new Date();
      // Ajustar la hora a Bogotá (si es necesario)
      return new Date(fechaActual.toLocaleString('en-US', { timeZone: 'America/Bogota' }));
    };
    let dias: number | undefined = this.membresiaSeleccionado?.duracionDias

    const calcularFechaVencimiento = (dias: number): string => {
      const fechaActual = obtenerFechaActualBogota();
      const opciones: Intl.DateTimeFormatOptions = {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        timeZone: 'America/Bogota',
      };

      if (dias < 2) {
        // Fecha de vencimiento es el mismo día
        return new Intl.DateTimeFormat('es-CO', opciones).format(fechaActual);
      } else if (dias >= 28 && dias <= 31) {
        // Sumar un mes
        fechaActual.setMonth(fechaActual.getMonth() + 1);
        return new Intl.DateTimeFormat('es-CO', opciones).format(fechaActual);
      } else {
        // Asumimos que es anual
        fechaActual.setFullYear(fechaActual.getFullYear() + 1);
        return new Intl.DateTimeFormat('es-CO', opciones).format(fechaActual);
      }
    };

    const duracionDias = this.membresiaSeleccionado?.duracionDias ?? 0;
    const fechaVencimiento = calcularFechaVencimiento(duracionDias);

    const _membresia: AsignacionMembresia = {

      asignacionId: this.datosMembresia == null ? 0 : this.datosMembresia.asignacionId,
      idMembresia: this.formularioMembresia.value.idMembresia,
      idUsuario: this.formularioMembresia.value.idUsuario,
      estado: "Pendiente",
      fechaVencimiento: fechaVencimiento


    };

    if (this.datosMembresia == null) {
      this._membresiaServicio.asignarMembresia(_membresia).subscribe({
        next: (data) => {
          if (data.status) {
            Swal.fire({
              icon: 'success',
              title: 'Membresia Registrada',
              text: `La membresia fue registrada`,
            });
            // this._utilidadServicio.mostrarAlerta("El producto fue registrado", "Exito");
            this.modalActual.close("true");
          } else {
            if (data.msg == "Ya existe una membresia con este nombre.") {
              Swal.fire({
                icon: 'error',
                title: 'ERROR',
                text: `Ya existe una membresia con este nombre.`,
              });


            } else {
              Swal.fire({
                icon: 'error',
                title: 'ERROR',
                text: `No se pudo guardar la membresia`,
              });

            }

            // this._utilidadServicio.mostrarAlerta("Ya existe un producto con ese mismo nombre", "Error");
          }
        },
        error: (error) => {


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





      });
    } else {
      this._membresiaServicio.asignarMembresia(_membresia).subscribe({
        next: (data) => {
          if (data.status) {
            Swal.fire({
              icon: 'success',
              title: 'Membresia Editada',
              text: `La membresia fue editada.`,
            });
            // this._utilidadServicio.mostrarAlerta("El producto fue registrado", "Exito");
            this.modalActual.close("true");
          } else {
            if (data.msg == "Ya existe una membresia con este nombre.") {
              Swal.fire({
                icon: 'error',
                title: 'ERROR',
                text: `Ya existe una membresia con este nombre.`,
              });
              return

            } else if (data.msg == "El usuario ya tiene una membresía activa.") {
              Swal.fire({
                icon: 'error',
                title: 'ERROR',
                text: `El usuario ya tiene una membresía activa.`,
              });
              return

            }

            else {
              Swal.fire({
                icon: 'error',
                title: 'ERROR',
                text: `No se pudo editar el producto`,
              });
              return
            }

            // this._utilidadServicio.mostrarAlerta("Ya existe un producto con ese mismo nombre", "Error");
          }
        },
        error: (e) => {

          console.error('Error es :', e);
          // Swal.fire({
          //   icon: 'error',
          //   title: 'ERROR',
          //   text: ` el cliente  editar`,
          // });
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


      });


    }
    // }


  }




}
