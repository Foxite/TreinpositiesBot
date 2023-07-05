export interface DiscordCurrentAuthorization {
  application: {
    id: string,
    name: string,
    icon: string,
    description: string,
    hook: boolean,
    bot_public: boolean,
    bot_require_code_grant: boolean,
    verify_key: string
  };
  scopes: string[];
  expires: Date;
  user: {
    id: string,
    username: string,
    global_name: string,
    avatar: string,
    discriminator: string,
    public_flags: number
    avatar_decoration: unknown
  };
}
