import { ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Membresia } from '../../../../Interfaces/membresia';
import { environment } from '../../../../../environments/environment';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MembresiaService } from '../../../../Services/membresia.service';
import { DomSanitizer } from '@angular/platform-browser';
import { UtilidadService } from '../../../../Reutilizable/utilidad.service';
import { UsuariosService } from '../../../../Services/usuarios.service';
import Swal from 'sweetalert2';
import * as CryptoJS from 'crypto-js';
import { AbstractControl } from '@angular/forms';
import { Observable, of } from 'rxjs';

@Component({
  selector: 'app-modal-membresia',
  templateUrl: './modal-membresia.component.html',
  styleUrl: './modal-membresia.component.css'
})
export class ModalMembresiaComponent  implements OnInit {


  formularioMembresia: FormGroup;

  tituloAccion: string = "Agregar";
  botonAccion: string = "Guardar";
  listaCategoria: Membresia[] = [];

  urlApi: string = environment.endpoint;


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

  clienteFiltrado: string = '';
  // Asegúrate de inicializar esta lista
  constructor(
    private modalActual: MatDialogRef<ModalMembresiaComponent>,
    @Inject(MAT_DIALOG_DATA) public datosMembresia: Membresia, private fb: FormBuilder,
    private _membresiaServicio: MembresiaService,
    private _utilidadServicio: UtilidadService, private sanitizer: DomSanitizer,
    private cdRef: ChangeDetectorRef,
    private _usuarioServicio: UsuariosService,
    private dialog: MatDialog,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {



    this.formularioMembresia = this.fb.group({

      nombre: ['', [Validators.required, Validators.maxLength(300)]],
      descripcion: ['', Validators.required,],
      precioTexto: ['', [Validators.required, Validators.maxLength(10)]],
      esActivo: [this.datosMembresia ? this.datosMembresia.esActivo?.toString() : '1'],
      duracionDias:[ '',
        Validators.required,
        Validators.min(1),
        Validators.max(365),
        Validators.pattern(/^\d{1,3}$/) // Asegura máximo 3 dígitos
      ],

    });

    if (datosMembresia != null) {
      this.tituloAccion = "Editar";
      this.botonAccion = "Actualizar";
      this.modoEdicion = true;
    }







  }



  ngOnInit(): void {



    if (this.datosMembresia != null) {
      const precioNumerico = parseFloat(this.datosMembresia.precioTexto);
      const precioFormateado = precioNumerico.toFixed(0);

      console.log("Valor de activo recibido:", this.datosMembresia);

      this.formularioMembresia.patchValue({

        nombre: this.datosMembresia.nombre,
        descripcion: this.datosMembresia.descripcion,
        precioTexto: precioFormateado,
        duracionDias: this.datosMembresia.duracionDias,
        // activo: this.datosMembresia.activo === 0 ? '0' : '1',
        esActivo: this.datosMembresia.esActivo!.toString(),

      })
      console.log("Valor de 'activo' después de patchValue:", this.formularioMembresia.get('activo')?.value);

    }

    console.log("Valor de 'activo' después de patchValue:", this.formularioMembresia.get('activo')?.value);


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






  letrasValidator() {
    return (control: FormControl) => {
      const nombre = control.value;
      const soloLetras = /^[a-zA-Z]+$/.test(nombre);
      return soloLetras ? null : { letrasValidator: true };
    };
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

    console.log("Estado del formulario:", this.formularioMembresia.status);

    // Verificar si el formulario es inválido
    if (this.formularioMembresia.invalid) {
      this._utilidadServicio.mostrarAlerta("Por favor, complete todos los campos correctamente", "Error");
      return;
    }



    let precioString = this.formularioMembresia.value.precioTexto;
    let activoNumber = this.formularioMembresia.value.esActivo;


      precioString = this.formularioMembresia.value.precioTexto|| "0";
      const precioSinPuntos = precioString.replace(/\./g, '');


    const _membresia: Membresia = {

      idMembresia: this.datosMembresia == null ? 0 : this.datosMembresia.idMembresia,
      nombre: this.formularioMembresia.value.nombre,
      precioTexto: precioSinPuntos,
      descripcion: this.formularioMembresia.value.descripcion,
      duracionDias: this.formularioMembresia.value.duracionDias,
      esActivo:  parseInt(activoNumber),


    };

    if (this.datosMembresia == null) {
      this._membresiaServicio.guardarMembresia(_membresia).subscribe({
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
      this._membresiaServicio.editarMembresia(_membresia).subscribe({
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


            } else {
              Swal.fire({
                icon: 'error',
                title: 'ERROR',
                text: `No se pudo editar el producto`,
              });

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

  onCategoriaSelected(option: any): void {
    this.categoriaSeleccionada = option.idCategoria;  // Guardar la categoría seleccionada

  }




  lastItem(item: any, list: any[]): boolean {
    return item === list[list.length - 1];
  }


}
