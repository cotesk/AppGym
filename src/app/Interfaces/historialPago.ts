export interface HistorialPago {

  historialPagoId:number,
  pagoId: number,
  idUsuario: number,
  nombreUsuario:string;
  fechaPago: string,
  montoTexto: string;
  tipoPago?: string;


}
