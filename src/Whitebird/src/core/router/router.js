class Router {
  constructor() {
    this.routes = new Map();
    this.guards = [];
    this.currentRoute = null;
    this.layoutComponent = null;
    
    window.addEventListener('popstate', () => this.handleRoute());
  }

  register(path, component, options = {}) {
    this.routes.set(path, {
      component,
      layout: options.layout || 'default',
      title: options.title || 'Whitebird',
      requiresAuth: options.requiresAuth || false,
      roles: options.roles || []
    });
    return this;
  }

  guard(guardFunction) {
    this.guards.push(guardFunction);
    return this;
  }

  setLayout(layoutComponent) {
    this.layoutComponent = layoutComponent;
  }

  async navigate(path, options = {}) {
    const url = new URL(path, window.location.origin);
    
    if (options.replace) {
      window.history.replaceState({}, '', url);
    } else {
      window.history.pushState({}, '', url);
    }
    
    await this.handleRoute();
  }

  async handleRoute() {
    const path = window.location.pathname;
    const route = this.findRoute(path);
    
    if (!route) {
      await this.navigate('/404', { replace: true });
      return;
    }

    // Run guards
    for (const guard of this.guards) {
      const result = await guard(route, path);
      if (result === false) {
        await this.navigate('/login', { replace: true });
        return;
      }
      if (typeof result === 'string') {
        await this.navigate(result, { replace: true });
        return;
      }
    }

    // Check auth requirement
    if (route.requiresAuth) {
      const token = localStorage.getItem('whitebird_session_token');
      if (!token) {
        await this.navigate('/login', { replace: true });
        return;
      }
    }

    this.currentRoute = { ...route, path };
    document.title = route.title;
    
    await this.render(route);
  }

  findRoute(path) {
    for (const [routePath, route] of this.routes) {
      const params = this.matchRoute(routePath, path);
      if (params !== null) {
        return { ...route, params };
      }
    }
    return null;
  }

  matchRoute(routePath, currentPath) {
    const routeParts = routePath.split('/');
    const currentParts = currentPath.split('/');
    
    if (routeParts.length !== currentParts.length) {
      return null;
    }
    
    const params = {};
    
    for (let i = 0; i < routeParts.length; i++) {
      if (routeParts[i].startsWith(':')) {
        params[routeParts[i].slice(1)] = currentParts[i];
      } else if (routeParts[i] !== currentParts[i]) {
        return null;
      }
    }
    
    return params;
  }

  async render(route) {
    const app = document.getElementById('app');
    
    if (!app) {
      console.error('App container not found');
      return;
    }

    try {
      const component = await route.component();
      const ComponentClass = component.default || component;
      
      let content;
      
      if (this.layoutComponent && route.layout !== 'none') {
        const Layout = await this.layoutComponent();
        const LayoutClass = Layout.default || Layout;
        const layoutInstance = new LayoutClass();
        content = layoutInstance.render(ComponentClass, route);
      } else {
        const instance = new ComponentClass();
        content = instance.render();
      }
      
      app.innerHTML = '';
      app.appendChild(content);
      
      // Call mounted lifecycle if exists
      const instance = new ComponentClass();
      if (instance.mounted) {
        await instance.mounted(route.params);
      }
    } catch (error) {
      console.error('Failed to render route:', error);
      app.innerHTML = '<div class="error-container"><h2>Failed to load page</h2></div>';
    }
  }

  back() {
    window.history.back();
  }

  forward() {
    window.history.forward();
  }

  getCurrentRoute() {
    return this.currentRoute;
  }

  getParams() {
    return this.currentRoute?.params || {};
  }
}

export default new Router();