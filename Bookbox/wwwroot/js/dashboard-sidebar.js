// Dashboard Sidebar Functionality
document.addEventListener('DOMContentLoaded', function() {
    const sidebar = document.getElementById('dashboardSidebar');
    const mainContent = document.getElementById('mainContent');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarCollapseBtn = document.getElementById('sidebarCollapseBtn');
    const sidebarCollapseInside = document.getElementById('sidebarCollapseInside');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    
    // Add data-title attributes to all sidebar links for tooltips
    const sidebarLinks = document.querySelectorAll('.sidebar-link');
    sidebarLinks.forEach(link => {
        const text = link.querySelector('span')?.textContent.trim() || '';
        if (text) {
            link.setAttribute('data-title', text);
        }
    });
    
    // Function to check and set sidebar state based on screen size
    function checkScreenSize() {
        if (window.innerWidth < 768) {
            // Mobile view - hide sidebar by default
            sidebar.classList.remove('show');
            sidebarOverlay.classList.remove('show');
        } else {
            // Desktop view - show sidebar
            if (mainContent) {
                mainContent.classList.remove('sidebar-collapsed');
            }
            
            // Check localStorage for persistent preference
            const sidebarState = localStorage.getItem('sidebarState');
            if (sidebarState === 'collapsed') {
                sidebar.classList.add('collapsed');
                if (mainContent) mainContent.classList.add('sidebar-collapsed');
                updateCollapseButtonIcon(true);
            } else {
                sidebar.classList.remove('collapsed');
                if (mainContent) mainContent.classList.remove('sidebar-collapsed');
                updateCollapseButtonIcon(false);
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
        const willBeCollapsed = !sidebar.classList.contains('collapsed');
        
        sidebar.classList.toggle('collapsed');
        if (mainContent) {
            mainContent.classList.toggle('sidebar-collapsed');
        }
        
        updateCollapseButtonIcon(willBeCollapsed);
        
        // Save preference to localStorage
        localStorage.setItem('sidebarState', willBeCollapsed ? 'collapsed' : 'expanded');
    }
    
    // Update the collapse button icon based on sidebar state
    function updateCollapseButtonIcon(isCollapsed) {
        const collapseIcon = sidebarCollapseInside.querySelector('.collapse-icon');
        
        if (isCollapsed) {
            collapseIcon.classList.remove('fa-angle-double-left');
            collapseIcon.classList.add('fa-angle-double-right');
        } else {
            collapseIcon.classList.remove('fa-angle-double-right');
            collapseIcon.classList.add('fa-angle-double-left');
        }
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
    
    if (sidebarCollapseInside) {
        sidebarCollapseInside.addEventListener('click', function() {
            toggleSidebarDesktop();
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