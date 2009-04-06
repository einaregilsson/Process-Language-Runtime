

#token types
LOCATION = 'LOCATION'
VARIABLE = 'VARIABLE'
NUMBER = 'NUMBER'
DOUBLESEMI = '::'
DOUBLEPIPE = '||'
LANGLE = '<'
RANGLE = '>'
DOT = '.'
LPAREN = '('
RPAREN = ')'
COMMA = ','
PLUS = '+'
MINUS = '-'
ASTERISK = '*'
DIVIDE = '/'
OUT = 'out'
IN = 'in'
READ = 'read'
AT = '@'
COMMENTSTART = '#'
COMMENTEND = '\n'
EOF = 'EOF'

class Token:
    def __init__(self, val, type, line, col):
        self.val = val
        self.type = type
        self.line = line
        self.col = col
        
    def __str__(self):
        return '%s (%d,%d)' % (self.val, self.line, self.col)
        
class Lexer:
    
    def __init__(self, buffer):
        self.buffer = buffer
        self.pos = 0
        self.line = 1
        self.col = 1
    
    def update_pos(self, ch):
        self.pos += 1
        self.col += 1
        if ch == '\n':
            self.line+=1
            self.col = 1
        
    
    def get_until(self, stop, include_stop_char=False):
        chars = []
        ch = self.buffer[self.pos]
        while self.pos < len(self.buffer) and not stop(self.buffer[self.pos]):
            ch = self.buffer[self.pos]
            chars.append(ch)
            self.update_pos(ch)
        
        if self.pos < len(self.buffer) and include_stop_char:
            chars.append(self.buffer[self.pos])
            self.update_pos(self.buffer[self.pos])
            
        return ''.join(chars)            
        
    def at_end(self):
        return self.pos >= len(self.buffer)
        
    def lex(self):
        if self.pos == len(self.buffer):
            return Token(EOF, EOF, self.line, self.col)
        while not self.at_end():
            ch = self.buffer[self.pos]
            l,c = self.line, self.col
            self.update_pos(ch)

            if ch.isspace():
                pass
            elif ch == COMMENTSTART:
                self.get_until(lambda c: c == COMMENTEND)
            elif ch in (DOT, LPAREN, RPAREN, LANGLE, RANGLE, COMMA, PLUS, MINUS, AT, ASTERISK, DIVIDE):           
                return Token(ch, ch, l, c)
            elif ch.isalpha() and ch.islower():
                val = ch + self.get_until(lambda c: not c.isalnum())
                if val in (IN, OUT, READ):
                    return Token(val, val, l,c)
                return Token(val, VARIABLE, l,c)
            elif ch.isalpha() and ch.isupper():
                val = ch + self.get_until(lambda c: not c.isalnum())
                return Token(val, LOCATION, l,c)
            elif ch.isdigit():
                val = int(ch + self.get_until(lambda c: not c.isdigit()))
                return Token(val, NUMBER, l,c)
            elif ch == ':' and not self.at_end() and self.buffer[self.pos] == ':':
                self.update_pos(self.buffer[self.pos])
                return Token(DOUBLESEMI, DOUBLESEMI, l, c)
            elif ch == ':' and self.buffer[self.pos-1:self.pos+1] == '::':
                self.update_pos(self.buffer[self.pos])
                return Token(DOUBLESEMI, DOUBLESEMI, l, c)
            elif ch == '|' and self.buffer[self.pos-1:self.pos+1] == '||':
                self.update_pos(self.buffer[self.pos])
                return Token(DOUBLEPIPE, DOUBLEPIPE, l, c)
            else:
                print 'Invalid token "%s" at (%d, %d)' % (ch, l, c)
            

class Parser:
    def __init__(self, lexer):
        self.lexer = lexer
        self.t = None
        self.la = self.lexer.lex()
        
    def get(self):
        self.t = self.la
        self.la = self.lexer.lex()

    def net(self):
        self.located_item()

        self.expect(self.la, DOUBLEPIPE, EOF)
        while self.la.type == DOUBLEPIPE:
            self.get()
            self.located_item()

    def expect(self, token, *expected):
            
        if all(token.type != exp for exp in expected):
            if len(expected) == 1:
                print "(%d, %d) Expected %s" % (token.line, token.col, expected[0])
            elif len(expected) == 2:
                print "(%d, %d) Expected %s or %s" % (token.line, token.col, expected[0], expected[1])
            elif len(expected) > 2:
                print "(%d, %d) Expected %s or %s" % (token.line, token.col, ', '.join(expected[:-1]), expected[-1])
    
    def located_item(self):
        self.get()
        self.expect(self.t, LOCATION)
        self.get()
        self.expect(self.t, DOUBLESEMI)
        if self.la.type == LANGLE:
            self.tuple()
        else:
            self.process()
        
    def tuple(self):
        self.get()
        self.expect(self.t, LANGLE)
        self.element()
        self.expect(self.la, COMMA, RANGLE)
        while self.la.type == COMMA:
            self.get()
            self.element()
        self.get()
        self.expect(self.t, RANGLE)

    def element(self):
        self.get()
        if self.t.type == VARIABLE:
            pass #process var
        elif self.t.type == LOCATION:
            pass #process loc
        elif self.t.type == NUMBER:
            pass #process number
        else:
            print '(%d, %d) Expected NUMBER, VARIABLE or LOCATION' % (self.t.line, self.t.col)
        
    def process(self):
        pass
        
                
        
        
        
if __name__ == '__main__':
    text = """
    Foo::<23, Foj, j>
    || FAS::<asdf ,s> 

    """
    p = Parser(Lexer(text))
    p.net()
#    tok = Token('begin','begin',21,3)
#    while tok:
#        print tok.val, ' type: ', tok.type, ' (%d,%d)' % (tok.line,tok.col)
#        tok = l.lex()
 