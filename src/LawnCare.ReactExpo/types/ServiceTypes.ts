export interface Service {
  serviceName: string;
  quantity: number;
  comment: string;
  price: number;
}

export interface ServiceRequest {
  userId: string;
  tenantId: string;
  customerFirstName: string;
  customerLastName: string;
  customerAddress1: string;
  customerAddress2: string;
  customerAddress3: string;
  customerCity: string;
  customerState: string;
  customerZip: string;
  customerHomePhone: string;
  customerCellPhone: string;
  customerEmail: string;
  scheduledDate: string;
  estimatedCost: number;
  estimatedDuration: number;
  description: string;
  services: Service[];
}

export interface ServiceFormData extends Omit<ServiceRequest, 'scheduledDate'> {
  scheduledDate: Date | null;
}