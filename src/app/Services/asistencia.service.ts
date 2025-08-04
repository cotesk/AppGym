import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

// Importar modelos de datos
import { AsitenciaPersonal } from './../Interfaces/asistenciaPersonal';
import { ReponseApi } from '../Interfaces/reponse-api';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AsistenciaPersonalService {

  private apiUrl: string = environment.endpoint + "AsistenciaPersonal/"

  // Función para obtener los headers con el token de autenticación
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem("authToken");
    if (!token) {
      console.error('No se encontró un token JWT en el almacenamiento local.');
      throw new Error('No se encontró un token JWT en el almacenamiento local.');
    }
    return new HttpHeaders({
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`
    });
  }

  constructor(private http:HttpClient) { }

  // Registrar asistencia
  registrarAsistencia(idUsuario: number): Observable<ReponseApi> {
    return this.http.post<ReponseApi>(
      `${this.apiUrl}Registrar?idUsuario=${idUsuario}`,
      { headers: this.getHeaders() }
    )
    .pipe(catchError(this.handleError));
  }


  listaPaginada(page: number = 1, pageSize: number = 5, searchTerm: string = ''): Observable<any> {
    const url = `${this.apiUrl}ListaPaginada?page=${page}&pageSize=${pageSize}&searchTerm=${searchTerm}`;
    return this.http.get<any>(url, { headers: this.getHeaders() }).pipe(
      catchError(this.handleError)
    );
  }
  // Listar todas las asistencias
  listarAsistencias(): Observable<ReponseApi> {
    return this.http.get<ReponseApi>(
      `${this.apiUrl}Lista`,
      { headers: this.getHeaders() }
    )
    .pipe(catchError(this.handleError));
  }

  // Consultar asistencias por usuario
  consultarAsistenciasPorUsuario(idUsuario: number): Observable<ReponseApi> {
    return this.http.get<ReponseApi>(
      `${this.apiUrl}Consultar/${idUsuario}`,
      { headers: this.getHeaders() }
    )
    .pipe(catchError(this.handleError));
  }

  // Manejo de errores
  // private handleError(error: any): Observable<never> {
  //   console.error('Ocurrió un error:', error);
  //   return throwError('Error al realizar la solicitud. Por favor, inténtalo de nuevo más tarde.');
  // }

  private handleError(error: any): Observable<never> {
    console.error('Ocurrió un error:', error);

    // Crear un objeto de error personalizado que contenga tanto el status como el mensaje
    const errorResponse = {
      status: error.status || 500,  // Si no existe un status, usamos 500 (error interno del servidor)
      message: error.message || 'Error al realizar la solicitud.',
      msg: error.error?.msg || 'Error desconocido', // Si el servidor envió un mensaje específico
    };

    // Regresar el error en el observable para que pueda ser manejado en la suscripción
    return throwError(() => errorResponse);
  }

}
