import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MasterService {

  constructor(private http: HttpClient,
  ) { }

  public getHeader() {
    const token = localStorage.getItem("token");
    const headers: { [key: string]: string } = {
      'Content-Type': 'application/json',
    };
    
    if (token) {
      headers['Authorization'] = `Bearer ${token.trim()}`;
    } else {
      console.warn('MasterService: Token không tồn tại trong localStorage');
    }
    
    return {
      headers: new HttpHeaders(headers),
    };
  }
  
  get<T>(url: string): Observable<T> {
    return this.http.get<T>(url, this.getHeader());
  }

  getBlob(url: string): Observable<Blob> {
    return this.http.get(url, {
      ...this.getHeader(),
      responseType: 'blob',
    });
  }

  getwithParam<T>(url: string, params: Record<string, string | string[] | number | boolean | null | undefined>): Observable<T> {
    let httpParams = new HttpParams();
    for (const [key, value] of Object.entries(params)) {
      if (value == null) continue;
      if (Array.isArray(value)) {
        for (const item of value) {
          if (item == null || item === '') continue;
          httpParams = httpParams.append(key, String(item));
        }
      } else {
        httpParams = httpParams.set(key, String(value));
      }
    }
    const httpOptions = {
      ...this.getHeader(),
      params: httpParams,
    };

    return this.http.get<T>(url, httpOptions);
  }

  post<T>(URL: string, body: any): Observable<T> {
    return this.http.post<T>(URL, body, this.getHeader())
  }

  /** POST trả về file nhị phân (Excel, PDF…) — cần gắn Authorization giống post JSON. */
  postBlob(URL: string, body: unknown): Observable<Blob> {
    return this.http.post(URL, body, {
      ...this.getHeader(),
      responseType: 'blob',
    });
  }

  put<T>(URL: string, body: any): Observable<T> {
    return this.http.put<T>(URL, body, this.getHeader())
  }

  delete<T>(URL: string): Observable<T> {
    return this.http.delete<T>(URL, this.getHeader())
  }

  deleteWithBody<T>(url: string, body: any): Observable<T> {
    const token = localStorage.getItem('token')
    const headers: { [key: string]: string } = {
      'Content-Type': 'application/json',
    }
    if (token) headers['Authorization'] = `Bearer ${token.trim()}`
    return this.http.delete<T>(url, {
      headers: new HttpHeaders(headers),
      body,
    })
  }

  patch<T>(URL: string, body: any): Observable<T> {
    return this.http.patch<T>(URL, body, this.getHeader())
  }

  postFormData<T>(url: string, formData: FormData): Observable<T> {
    const token = localStorage.getItem('token')
    const headers: { [key: string]: string } = {}
    if (token) headers['Authorization'] = `Bearer ${token.trim()}`
    return this.http.post<T>(url, formData, {
      headers: new HttpHeaders(headers),
    })
  }

}
