import { NgbModal } from '@ng-bootstrap/ng-bootstrap'
import { ErrorModalComponent } from '@components/error-modal/error-modal.component'

export function interpolateString(str: string, params: any): string {
    return str.replace(/{{(\w+)}}/g, (match, p1) => params[p1] || '');
}

export function sumwidthConfig(widthConfig: any[]): string {
    let sum = 0;
    widthConfig.forEach(element => {
        let width = +element.substr(0, element.length - 2);
        sum += width
    });
    return sum + 'px';
}

/**
 * Mở ErrorModalComponent — dùng chung cho tất cả component.
 * @param modal NgbModal instance (inject từ constructor)
 * @param title Tiêu đề modal
 * @param message Nội dung lỗi
 */
export function showErrorModal(modal: NgbModal, title: string, message: string): void {
    const ref = modal.open(ErrorModalComponent, {
        backdrop: 'static',
        centered: true,
        size: 'sm',
        windowClass: 'error-modal-top',
    })
    ref.componentInstance.title = title
    ref.componentInstance.message = message
}