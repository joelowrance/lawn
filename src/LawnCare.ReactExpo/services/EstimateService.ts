import { API_BASE_URL } from '@env';
import { ServiceRequest } from '@/types/ServiceTypes';

export interface EstimateApiResponse {
  success: boolean;
  message?: string;
  data?: any;
}

export class EstimateService {
  private static baseUrl = API_BASE_URL;

  static async createEstimate(estimateData: ServiceRequest): Promise<EstimateApiResponse> {
    try {
      const response = await fetch(`${this.baseUrl}/estimate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(estimateData),
      });

      const responseData = await response.json();

      if (!response.ok) {
        return {
          success: false,
          message: responseData.message || `HTTP error! status: ${response.status}`,
          data: responseData,
        };
      }

      return {
        success: true,
        message: 'Estimate created successfully',
        data: responseData,
      };
    } catch (error) {
      console.error('Error creating estimate:', error);
      return {
        success: false,
        message: error instanceof Error ? error.message : 'Unknown error occurred',
      };
    }
  }
}