// Dashboard Sidebar Functionality
document.addEventListener('DOMContentLoaded', function() {
    const sidebar = document.getElementById('dashboardSidebar');
    const mainContent = document.getElementById('mainContent');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarCollapseBtn = document.getElementById('sidebarCollapseBtn');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    
    // Function to check and set sidebar state based on screen size
    function checkScreenSize() {
        if (window.innerWidth < 768) {
            // Mobile view - hide sidebar by default
            sidebar.classList.remove('show');
            sidebarOverlay.classList.remove('show');
        } else {
            // Desktop view - show sidebar
            sidebar.classList.remove('collapsed');
            if (mainContent) {
                mainContent.classList.remove('sidebar-collapsed');
            }
            
            // Check localStorage for persistent preference
            const sidebarState = localStorage.getItem('sidebarState');
            if (sidebarState === 'collapsed') {
                toggleSidebarDesktop();
            }
        }
    }
    
    // Toggle sidebar on mobile
    function toggleSidebarMobile() {
        sidebar.classList.toggle('show');
        sidebarOverlay.classList.toggle('show');
    }
    
    // Toggle sidebar on desktop
    function toggleSidebarDesktop() {
        sidebar.classList.toggle('collapsed');
        if (mainContent) {
            mainContent.classList.toggle('sidebar-collapsed');
        }
        
        // Save preference to localStorage
        const isCollapsed = sidebar.classList.contains('collapsed');
        localStorage.setItem('sidebarState', isCollapsed ? 'collapsed' : 'expanded');
    }
    
    // Event listeners
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            toggleSidebarMobile();
        });
    }
    
    if (sidebarCollapseBtn) {
        sidebarCollapseBtn.addEventListener('click', function() {
            if (window.innerWidth < 768) {
                toggleSidebarMobile();
            } else {
                toggleSidebarDesktop();
            }
        });
    }
    
    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function() {
            toggleSidebarMobile();
        });
    }
    
    // Handle window resize
    window.addEventListener('resize', checkScreenSize);
    
    // Initialize sidebar state
    checkScreenSize();
});