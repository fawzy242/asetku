class Router {
  constructor() {
    this.routes = new Map();
    this.guards = [];
    this.currentRoute = null;
    this.layoutComponent = null;
    this.root = null;
    
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
      const componentModule = await route.component();
      const ComponentClass = componentModule.default || componentModule;
      
      const React = await import('react');
      const ReactDOMClient = await import('react-dom/client');
      
      // Create root only once
      if (!this.root) {
        this.root = ReactDOMClient.createRoot(app);
      }
      
      const componentProps = { params: route.params };
      
      if (this.layoutComponent && route.layout !== 'none') {
        const layoutModule = await this.layoutComponent();
        const LayoutClass = layoutModule.default || layoutModule;
        
        this.root.render(
          React.createElement(
            LayoutClass,
            { title: route.title },
            React.createElement(ComponentClass, componentProps)
          )
        );
      } else {
        this.root.render(React.createElement(ComponentClass, componentProps));
      }
      
    } catch (error) {
      console.error('Failed to render route:', error);
      app.innerHTML = `
        <div style="display: flex; align-items: center; justify-content: center; min-height: 100vh; flex-direction: column; font-family: sans-serif; padding: 20px; text-align: center;">
          <h1 style="color: #dc2626; font-size: 2rem; margin-bottom: 1rem;">Failed to load page</h1>
          <p style="color: #6b7280; margin-bottom: 1rem;">${error.message}</p>
          <button onclick="window.location.href='/dashboard'" style="padding: 0.5rem 1rem; background: #dc2626; color: white; border: none; border-radius: 0.375rem; cursor: pointer;">
            Go to Dashboard
          </button>
        </div>
      `;
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