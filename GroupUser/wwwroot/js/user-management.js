// Hàm mở modal với các chế độ khác nhau
function openUserModal(mode, id, username, fullName, dateOfBirth, gender, phoneNumber, email, groupId) {
    // Reset toàn bộ form và trạng thái controls
    $('#UserForm')[0].reset();
    $('#UserForm input, #UserForm select').prop('disabled', false);

    // Set tiêu đề modal
    $('#UserModalLabel').text(
        mode === 'EditUser' ? 'Sửa User' :
            mode === 'CreateUser' ? 'Thêm User mới' : 'Chi tiết User'
    );

    // Xử lý riêng cho chế độ Create
    if (mode === 'CreateUser') {
        $('#userId').val('');
        $('#saveButton').show();
        return $('#UserModal').modal('show');
    }

    // Điền giá trị vào form
    $('#userId').val(id);
    $('#Username').val(username);
    $('#FullName').val(fullName);

    // Xử lý ngày sinh
    if (dateOfBirth) {
        const dateObj = new Date(dateOfBirth);
        if (!isNaN(dateObj.getTime())) {
            $('#DateOfBirth').val(dateObj.toISOString().split('T')[0]);
        }
    }

    $('#Gender').val(gender ? 'true' : 'false');
    $('#PhoneNumber').val(phoneNumber);
    $('#Email').val(email);
    $('#parentGroupId').val(groupId?.toString() || '');

    // Xử lý trạng thái form theo mode
    if (mode === 'DetailUser') {
        $('#saveButton').hide();
        $('#UserForm input, #UserForm select').prop('disabled', true);
    } else {
        $('#saveButton').show();
        $('#UserForm input, #UserForm select').prop('disabled', false);
    }

    $('#UserModal').modal('show');
}

// Hàm cập nhật bảng user
function updateUserTable(users) {
    var tbody = $('table tbody');
    tbody.empty();

    users.forEach(function (user, index) {
        var dateOfBirth = new Date(user.dateOfBirth);
        var formattedDate = dateOfBirth.getDate().toString().padStart(2, '0') + '/' +
            (dateOfBirth.getMonth() + 1).toString().padStart(2, '0') + '/' +
            dateOfBirth.getFullYear();
        var row = `
            <tr data-id="${user.id}">
                <td>${index + 1}</td>
                <td>${user.username}</td>
                <td>${user.fullName}</td>
                <td>${formattedDate}</td>
                <td>${user.gender ? 'Nam' : 'Nữ'}</td>
                <td>${user.phoneNumber}</td>
                <td>${user.email}</td>
                <td>${user.groupName ? user.groupName : user.groupId}</td>
                <td>
                    <button class="btn btn-info btn-sm" onclick="openUserModal('DetailUser', '${user.id}', '${user.username}', '${user.fullName}', '${user.dateOfBirth}', '${user.gender}', '${user.phoneNumber}', '${user.email}', '${user.groupId}')">Detail</button>
                    <button class="btn btn-warning btn-sm" onclick="openUserModal('EditUser', '${user.id}', '${user.username}', '${user.fullName}', '${user.dateOfBirth}', '${user.gender}', '${user.phoneNumber}', '${user.email}', '${user.groupId}')">Edit</button>
                    <button class="btn btn-danger btn-sm" onclick="confirmUserDelete(event, '${user.id}')">Delete</button>
                </td>
            </tr>
        `;
        tbody.append(row);
    });
}

// Hàm xác nhận xóa user
function confirmUserDelete(event, userId) {
    Swal.fire({
        title: 'Bạn có chắc muốn xóa người dùng này không?',
        text: 'Dữ liệu sẽ bị mất vĩnh viễn.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Có, xóa!',
        cancelButtonText: 'Không, hủy bỏ',
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/User/Delete/' + userId,
                type: 'POST',
                success: function (response) {
                    if (response.success) {
                        $('tr[data-id="' + userId + '"]').remove();
                        Swal.fire({
                            icon: 'success',
                            title: 'Đã xóa',
                            text: response.message,
                            timer: 2000,
                            showConfirmButton: false
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Lỗi',
                            text: response.message
                        });
                    }
                },
                error: function (xhr) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Đã xảy ra lỗi khi xóa người dùng'
                    });
                }
            });
        }
    });
}

// Hàm load user theo group
function loadUsersByGroup(groupId) {
    $.ajax({
        url: '/User/GetUsersByGroup/' + groupId,
        type: 'GET',
        success: function (users) {
            updateUserTable(users);
        },
        error: function (xhr, status, error) {
            console.error('Error loading users:', error);
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: 'Không thể tải danh sách người dùng'
            });
        }
    });
}

// Hàm load tất cả user
function loadAllUsers() {
    $('#userTableBody').html('<tr><td colspan="9" class="text-center"><i class="fas fa-spinner fa-spin"></i> Đang tải dữ liệu người dùng...</td></tr>');

    $.ajax({
        url: '/User/GetAllUsers',
        type: 'GET',
        success: function (users) {
            if (users && users.length > 0) {
                updateUserTable(users);
            } else {
                $('#userTableBody').html('<tr><td colspan="9" class="text-center">Không có người dùng nào</td></tr>');
            }
        },
        error: function (xhr, status, error) {
            console.error('Lỗi khi tải người dùng:', error);
            $('#userTableBody').html('<tr><td colspan="9" class="text-center text-danger">Lỗi khi tải dữ liệu người dùng</td></tr>');
        }
    });
}

// Hàm hiển thị xác nhận
function showConfirmation(event, formId, title) {
    if (event) event.preventDefault();

    Swal.fire({
        icon: 'warning',
        title: title,
        showCancelButton: true,
        confirmButtonText: 'Vâng!',
        cancelButtonText: 'Hủy',
        confirmButtonColor: '#f59e0b',
        cancelButtonColor: '#ef4444',
        customClass: {
            popup: 'swal2-border-radius'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            if (typeof formId === 'string') {
                document.getElementById(formId).submit();
            } else if (typeof formId === 'object') {
                formId.submit();
            }
        }
    });
}

// Hàm mở modal group
function openModal(mode, id = '', code = '', name = '', parentId = '') {
    const modalTitle = document.getElementById("groupModalLabel");
    const groupForm = document.getElementById("groupForm");
    const saveButtonGroup = document.getElementById("saveButtonGroup");
    const inputs = document.querySelectorAll('#groupForm input, #groupForm select, #groupForm button');

    // Đặt giá trị cho các trường
    document.getElementById("groupId").value = id;
    document.getElementById("GroupCode").value = code;
    document.getElementById("groupName").value = name;
    document.getElementById("parentGroupId").value = parentId;

    let formAction = "";
    let title = "";

    if (mode === 'CreateGroup') {
        modalTitle.innerText = "Create New Group";
        formAction = "/Group/CreateGroup";
        title = "Bạn có muốn thêm nhóm không?";
        inputs.forEach(input => {
            input.disabled = false;
            input.classList.remove('bg-light');
        });
        saveButtonGroup.style.display = "block";
    } else if (mode === 'edit') {
        modalTitle.innerText = "Edit Group";
        formAction = "/Group/EditGroup/" + id;
        title = "Bạn có muốn cập nhật nhóm không?";
        inputs.forEach(input => {
            input.disabled = false;
            input.classList.remove('bg-light');
        });
        saveButtonGroup.style.display = "block";
    } else if (mode === 'detail') {
        modalTitle.innerText = "Group Details";
        inputs.forEach(input => {
            input.disabled = true;
            input.classList.add('bg-light');
        });
        saveButtonGroup.style.display = "none";
        $('#groupModal').modal('show');
        return;
    }

    groupForm.action = formAction;
    saveButtonGroup.onclick = null;
    saveButtonGroup.addEventListener('click', function (event) {
        showConfirmation(event, "groupForm", title);
    });

    $('#groupModal').modal('show');
}

// Hàm xác nhận xóa group
function confirmDelete(event, id) {
    var form = document.createElement('form');
    form.method = 'post';
    form.action = '/Group/DeleteGroup/' + id;
    form.id = 'deleteForm-' + id;

    var token = document.createElement('input');
    token.type = 'hidden';
    token.name = '__RequestVerificationToken';
    token.value = $('input[name="__RequestVerificationToken"]').val();
    form.appendChild(token);

    document.body.appendChild(form);

    showConfirmation(event, form, "Bạn có chắc chắn muốn xóa nhóm này?");
}

// Document ready
$(document).ready(function () {
    // Xử lý form user
    $('#UserForm').on('submit', function (event) {
        event.preventDefault();

        // Lấy giá trị của các trường trong form
        var username = $('#Username').val().trim();
        var fullName = $('#FullName').val().trim();
        var dateOfBirth = $('#DateOfBirth').val().trim();
        var phoneNumber = $('#PhoneNumber').val().trim();
        var email = $('#Email').val().trim();

        // Biến kiểm tra lỗi
        var isValid = true;
        var errorMessage = '';

        // Validate Username: không trống, từ 10-50 ký tự
        if (username === '' || username.length < 5 || username.length > 50) {
            isValid = false;
            errorMessage += 'Username phải từ 5 đến 50 ký tự.\n';
        }

        // Validate FullName: không trống, từ 5-50 ký tự
        if (fullName === '' || fullName.length < 5 || fullName.length > 50) {
            isValid = false;
            errorMessage += 'FullName phải từ 5 đến 50 ký tự.\n';
        }

        // Validate DateOfBirth: không trống, không nhỏ hơn 1900 và không lớn hơn ngày hiện tại
        var birthDate = new Date(dateOfBirth);
        var currentDate = new Date();
        if (dateOfBirth === '' || birthDate.getFullYear() < 1900 || birthDate > currentDate) {
            isValid = false;
            errorMessage += 'Ngày sinh không hợp lệ.\n';
        }

        // Validate PhoneNumber: không trống, bắt buộc 10 số và phải bắt đầu với 03 hoặc 09
        var phoneRegex = /^(03|09)[0-9]{8}$/;
        if (phoneNumber === '' || !phoneRegex.test(phoneNumber)) {
            isValid = false;
            errorMessage += 'Số điện thoại phải bắt đầu bằng 03 hoặc 09 và có đúng 10 chữ số.\n';
        }

        // Nếu có lỗi, hiển thị thông báo lỗi
        if (!isValid) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: errorMessage
            });
            return;
        }

        // Nếu tất cả đều hợp lệ, tiếp tục với ajax request
        const mode = $('#userId').val() ? 'EditUser' : 'CreateUser';
        const actionText = mode === 'EditUser' ? 'sửa' : 'thêm';

        // Hiển thị thông báo xác nhận
        Swal.fire({
            title: `Bạn có muốn ${actionText} người dùng này không?`,
            text: `Thông tin sẽ được lưu sau khi xác nhận.`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: `Có, ${actionText}!`,
            cancelButtonText: 'Không, hủy bỏ',
        }).then((result) => {
            if (result.isConfirmed) {
                var formData = new FormData(this);
                const url = mode === 'EditUser' ? '/User/EditUser' : '/User/CreateUser';
                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (response) {
                        if (response.success) {
                            // Đóng modal và reset form
                            $('#UserModal').modal('hide');
                            $('.modal-backdrop').remove();
                            $('body').removeClass('modal-open');

                            // Format ngày sinh
                            var dateOfBirth = new Date(response.user.dateOfBirth);
                            var formattedDate = dateOfBirth.getDate().toString().padStart(2, '0') + '/' +
                                (dateOfBirth.getMonth() + 1).toString().padStart(2, '0') + '/' +
                                dateOfBirth.getFullYear();

                            if (mode === 'CreateUser') {
                                // Thêm mới: Thêm dòng mới vào bảng
                                var rowCount = $('#userTableBody tr').length;
                                var newIndex = rowCount + 1;

                                var newRow = `
                                    <tr data-id="${response.user.id}">
                                        <td>${newIndex}</td>
                                        <td>${response.user.username}</td>
                                        <td>${response.user.fullName}</td>
                                        <td>${formattedDate}</td>
                                        <td>${response.user.gender ? 'Nam' : 'Nữ'}</td>
                                        <td>${response.user.phoneNumber}</td>
                                        <td>${response.user.email}</td>
                                        <td>${response.user.groupId}</td>
                                        <td>
                                            <button class="btn btn-info btn-sm" onclick="openUserModal('DetailUser', '${response.user.id}', '${response.user.username}', '${response.user.fullName}', '${response.user.dateOfBirth}', '${response.user.gender}', '${response.user.phoneNumber}', '${response.user.email}', '${response.user.groupId}')">Detail</button>
                                            <button class="btn btn-warning btn-sm" onclick="openUserModal('EditUser', '${response.user.id}', '${response.user.username}', '${response.user.fullName}', '${response.user.dateOfBirth}', '${response.user.gender}', '${response.user.phoneNumber}', '${response.user.email}', '${response.user.groupId}')">Edit</button>
                                            <button class="btn btn-danger btn-sm" onclick="confirmUserDelete(event, '${response.user.id}')">Delete</button>
                                        </td>
                                    </tr>
                                `;

                                if ($('#userTableBody tr td').text().trim() === 'Không có người dùng nào') {
                                    $('#userTableBody').html(newRow);
                                } else {
                                    $('#userTableBody').append(newRow);
                                }
                            } else {
                                // Sửa: Cập nhật dòng hiện có
                                const row = $(`#userTableBody tr[data-id="${response.user.id}"]`);
                                row.find('td:eq(1)').text(response.user.username);
                                row.find('td:eq(2)').text(response.user.fullName);
                                row.find('td:eq(3)').text(formattedDate);
                                row.find('td:eq(4)').text(response.user.gender ? 'Nam' : 'Nữ');
                                row.find('td:eq(5)').text(response.user.phoneNumber);
                                row.find('td:eq(6)').text(response.user.email);
                                row.find('td:eq(7)').text(response.user.groupId);

                                // Cập nhật lại các nút action
                                row.find('td:eq(8)').html(`
                                    <button class="btn btn-info btn-sm" onclick="openUserModal('DetailUser', '${response.user.id}', '${response.user.username}', '${response.user.fullName}', '${response.user.dateOfBirth}', '${response.user.gender}', '${response.user.phoneNumber}', '${response.user.email}', '${response.user.groupId}')">Detail</button>
                                    <button class="btn btn-warning btn-sm" onclick="openUserModal('EditUser', '${response.user.id}', '${response.user.username}', '${response.user.fullName}', '${response.user.dateOfBirth}', '${response.user.gender}', '${response.user.phoneNumber}', '${response.user.email}', '${response.user.groupId}')">Edit</button>
                                    <button class="btn btn-danger btn-sm" onclick="confirmUserDelete(event, '${response.user.id}')">Delete</button>
                                `);
                            }

                            // Hiển thị thông báo thành công
                            Swal.fire({
                                icon: 'success',
                                title: 'Thành công',
                                text: response.message,
                                timer: 2000,
                                showConfirmButton: false
                            });

                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Lỗi',
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: 'error',
                            title: 'Lỗi',
                            text: `Đã xảy ra lỗi khi ${actionText} người dùng`
                        });
                    }
                });
            }
        });
    });

    // Khởi tạo cây thư mục
    $.ajax({
        url: '@Url.Action("GetGroupTree")',
        type: 'GET',
        success: function (data) {
            var treeData = [];
            var parentCounter = 1;

            function setSTTAndBuildTree(groups, parentId = "#") {
                groups.forEach(function (group) {
                    var isParent = group.parent === null || group.parent === "#";

                    var currentGroup = {
                        id: group.id,
                        parent: group.parent === null ? "#" : group.parent,
                        text: isParent ? `${parentCounter++}. ${group.text}` : group.text,
                        icon: "fa fa-folder",
                        state: { opened: false }
                    };

                    treeData.push(currentGroup);

                    var childGroups = data.filter(g => g.parent === group.id);
                    if (childGroups.length > 0) {
                        setSTTAndBuildTree(childGroups, group.id);
                    }
                });
            }

            setSTTAndBuildTree(data);

            $('#groupTree').jstree({
                core: {
                    data: treeData,
                    themes: {
                        dots: false,
                        stripes: false
                    }
                },
                plugins: ["types", "contextmenu", "changed"],
                types: {
                    default: {
                        icon: "fa fa-folder",
                        li_attr: {
                            style: "color: #deae36; list-style-type: none;"
                        },
                        a_attr: {
                            style: "color: #deae36; text-decoration: none; padding-left: 5px;"
                        }
                    }
                },
                contextmenu: {
                    items: function ($node) {
                        return {
                            edit: {
                                label: "Sửa",
                                icon: "fa fa-edit",
                                _class: "edit-action",
                                action: function (data) {
                                    var inst = $.jstree.reference(data.reference);
                                    var obj = inst.get_node(data.reference);
                                    var groupId = obj.id;
                                    var groupName = obj.text;
                                    var parentId = obj.parent !== '#' ? obj.parent : '';
                                    var groupCode = obj.original && obj.original.data ? obj.original.data.code : 'CD12345';

                                    openModal('edit', groupId, groupCode, groupName, parentId);
                                    $('#groupModal').modal('show');
                                }
                            },
                            detail: {
                                label: "Chi tiết",
                                icon: "fa fa-info-circle",
                                _class: "detail-action",
                                action: function (data) {
                                    var inst = $.jstree.reference(data.reference);
                                    var obj = inst.get_node(data.reference);
                                    var groupId = obj.id;
                                    var groupName = obj.text;
                                    var parentId = obj.parent !== '#' ? obj.parent : '';
                                    var groupCode = obj.original && obj.original.data ? obj.original.data.code : 'CD12345';

                                    openModal('detail', groupId, groupCode, groupName, parentId);
                                    $('#groupModal').modal('show');
                                }
                            },
                            delete: {
                                label: "Xóa",
                                icon: "fa fa-trash",
                                _class: "delete-action",
                                action: function (data) {
                                    var inst = $.jstree.reference(data.reference);
                                    var obj = inst.get_node(data.reference);
                                    var groupId = obj.id;

                                    confirmDelete(null, groupId);
                                }
                            }
                        };
                    }
                }
            }).on("select_node.jstree", function (e, data) {
                var groupId = data.node.id;
                loadUsersByGroup(groupId);
            }).on("loaded.jstree", function () {
                var defaultGroupId = $('#groupTree').jstree("get_node", "#").id;
                loadUsersByGroup(defaultGroupId);
            });
        }
    });

    // Tự động ẩn alert sau 2.5 giây
    setTimeout(() => {
        const alert = document.querySelector('.alert');
        if (alert) {
            alert.classList.remove('show');
            alert.classList.add('fade');
            setTimeout(() => alert.remove(), 500);
        }
    }, 2500);
});