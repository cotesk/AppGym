import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { Membresia } from '../Interfaces/membresia';
import { ReponseApi } from '../Interfaces/reponse-api';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class MembresiaService {
  private urlApi: string = environment.endpoint + 'Membresia/';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('authToken');
    if (!token) {
      console.error('No se encontró un token JWT en el almacenamiento local.');
      throw new Error('No se encontró un token JWT en el almacenamiento local.');
    }
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    });
  }

  // Listar todas las membresías
  listarMembresias(): Observable<ReponseApi> {
    const headers = this.getHeaders();
    return this.http.get<ReponseApi>(`${this.urlApi}Lista`, { headers }).pipe(
      catchError(this.handleError)
    );
  }

  // Guardar una nueva membresía
  guardarMembresia(membresia: Membresia): Observable<ReponseApi> {
    const headers = this.getHeaders();
    return this.http
      .post<ReponseApi>(`${this.urlApi}Guardar`, membresia, { headers })
      .pipe(catchError(this.handleError));
  }

  // Editar una membresía existente
  editarMembresia(membresia: Membresia): Observable<ReponseApi> {
    const headers = this.getHeaders();
    return this.http
      .put<ReponseApi>(`${this.urlApi}Editar`, membresia, { headers })
      .pipe(catchError(this.handleError));
  }

  // Eliminar una membresía por ID
  eliminarMembresia(id: number): Observable<ReponseApi> {
    const headers = this.getHeaders();
    return this.http
      .delete<ReponseApi>(`${this.urlApi}Eliminar/${id}`, { headers })
      .pipe(catchError(this.handleError));
  }

  // Asignar una membresía a un usuario
  asignarMembresia(asignacion: any): Observable<ReponseApi> {
    const headers = this.getHeaders();
    return this.http
      .post<ReponseApi>(`${this.urlApi}Asignar`, asignacion, { headers })
      .pipe(catchError(this.handleError));
  }

  // Listar todas las asignaciones
  listarAsignaciones(): Observable<ReponseApi> {
    const headers = this.getHeaders();
    return this.http
      .get<ReponseApi>(`${this.urlApi}Asignaciones/Lista`, { headers })
      .pipe(catchError(this.handleError));
  }

  // Obtener membresía asignada por usuario
  getMembresiaByUsuario(idUsuario: number): Observable<any> {
    const headers = this.getHeaders();
    return this.http
      .get<any>(`${this.urlApi}GetMembresiaByUsuario/${idUsuario}`, { headers })
      .pipe(catchError(this.handleError));
  }

  getMembresiaByUsuario2(idUsuario: number): Observable<any> {

    return this.http
      .get<any>(`${this.urlApi}GetMembresiaByUsuario/${idUsuario}`)
      .pipe(catchError(this.handleError));
  }

  // Actualizar estados de asignaciones
  actualizarEstadoAsignaciones(): Observable<ReponseApi> {
    // const headers = this.getHeaders();
    return this.http
      .get<ReponseApi>(`${this.urlApi}FechasVencidas`, {})
      .pipe(catchError(this.handleError));
  }

  GetFechasVencidas(): Observable<ReponseApi> {
    // const headers = this.getHeaders();
    return this.http
      .get<ReponseApi>(`${this.urlApi}FechasVencidasUsuarios`, {})
      .pipe(catchError(this.handleError));
  }
  // Manejo de errores
  private handleError(error: any) {
    console.error('Error en la solicitud:', error);
    return throwError('Ocurrió un error en la solicitud. Por favor, inténtelo de nuevo más tarde.');
  }
}
