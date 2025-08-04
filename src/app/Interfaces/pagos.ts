export interface Pagos {


  pagoId: number,
  idUsuario: number,
  idAsistencia:number;
  fechaPago: string,
  montoTexto: string;
  metodoPago: string;
  tipoPago?: string;
  observaciones: string;

}
