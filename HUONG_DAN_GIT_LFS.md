# Hướng Dẫn Sửa Lỗi Thiếu File 3D (Đỏ File / Hình Hộp Xanh) Bằng Git LFS

Dự án này sử dụng **Git LFS (Large File Storage)** để quản lý các tệp tin 3D lớn (như các mô hình nhân vật `.fbx` và `.glb` vượt quá giới hạn 100MB của GitHub).

Nếu bạn hoặc bạn bè của bạn tải dự án về mà gặp lỗi **chữ đỏ trong Unity**, hoặc các mô hình nhân vật bị hiển thị dưới dạng **hình hộp màu xanh** (file bị hỏng/thiếu), đó là do Git chưa tải về các tệp tin nhị phân thực tế mà chỉ mới tải về các tệp tin text pointer (tệp tin trỏ liên kết).

Hãy làm theo các bước dưới đây để sửa lỗi này rất đơn giản:

---

## 🛠️ Hướng Dẫn Sửa Lỗi Nhanh (Chỉ làm 1 lần)

### Bước 1: Tải và cài đặt Git LFS trên máy tính
Nếu máy của bạn chưa cài đặt Git LFS, Git sẽ không thể tải được file mô hình thực tế.
1. Truy cập trang chủ Git LFS: [https://git-lfs.com/](https://git-lfs.com/)
2. Bấm **Download** và cài đặt tệp tin vừa tải về (chỉ cần bấm Next cho đến khi hoàn thành).

### Bước 2: Kích hoạt LFS trong thư mục dự án
1. Mở cửa sổ **Terminal** (CMD, PowerShell, hoặc Git Bash) tại thư mục gốc của dự án `PRU213` trên máy bạn.
2. Chạy lệnh sau để kích hoạt:
   ```bash
   git lfs install
   ```
   *(Nếu thấy thông báo `Git LFS initialized.` là thành công).*

### Bước 3: Đồng bộ và tải các file 3D thực tế về máy
Chạy lệnh sau để yêu cầu Git tải về toàn bộ các tệp tin 3D nhị phân dung lượng lớn:
```bash
git lfs pull
```
hoặc:
```bash
git lfs checkout
```
*Hệ thống sẽ tải xuống khoảng 1.4 GB dữ liệu mô hình thực tế và tự động ghi đè lên thư mục Unity của bạn.*

### Bước 4: Mở lại Unity
* Mở hoặc click chuột vào cửa sổ Unity Editor.
* Unity sẽ tự động phát hiện các tệp tin `.fbx` và `.glb` đã được khôi phục đầy đủ dữ liệu và tiến hành **Re-import (nhập lại)**.
* Chờ vài giây, tất cả lỗi đỏ và hình hộp xanh sẽ biến mất hoàn toàn!

---

## 📦 Cách Tải (Clone) Dự Án Đúng Chuẩn Từ Đầu

Để tránh bị lỗi ngay từ đầu khi tải dự án về, bạn nên sử dụng lệnh Git Clone thay vì tải file ZIP trực tiếp.

1. Hãy chắc chắn máy bạn đã cài Git LFS (Bước 1 ở trên).
2. Mở Terminal và clone dự án bằng lệnh:
   ```bash
   git clone -b vy https://github.com/DuongDinhKhoi-2607/PRU213.git
   ```
   *(Git sẽ tự động tải cả code và toàn bộ tệp tin LFS lớn cùng một lúc, tải xong mở Unity lên là chơi được ngay mà không cần chạy thêm lệnh nào khác).*
