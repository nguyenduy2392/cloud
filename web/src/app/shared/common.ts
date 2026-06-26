export function GetUser() {
    let user = localStorage.getItem("user");
    if (user) {
        return JSON.parse(user);
    }
    return {
        id: "ea740ce0-eda3-40e1-8ef7-8b7900c9e95e",
        status: 1,
        userName: "admin",
        email: "admin@happyecotech.com",
        avatar: " ",
        birthday: null,
        gender: 1,
        fullName: "Quản trị viên",
        password: "",
        address: "Kiến An, Hải Phòng",
        phone: "0961000001",
        mobile: null,
        yahoo: null,
        skype: null,
        facebook: null,
        detail: null,
        skin: null,
        lastLogin: null,
        parrentId: "ea740ce0-eda3-40e1-8ef7-8b7900c9e95e",
        companyId: null,
        isRootAdmin: true
    };
}

export function IsRootAdmin(): boolean {
    const user = GetUser();
    return !!user?.isRootAdmin;
}

export function getAvatarColor(name: string): string {
    if (!name) return '#6554c0';
    const colors = [
        '#E5534B', '#FF991F', '#22C55E', '#0065FF', '#8B5CF6',
        '#EC4899', '#06B6D4', '#F59E0B', '#10B981', '#6366F1'
    ];
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
        hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colors[Math.abs(hash) % colors.length];
}